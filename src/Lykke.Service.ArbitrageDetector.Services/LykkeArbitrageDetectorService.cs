﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class LykkeArbitrageDetectorService : ILykkeArbitrageDetectorService, IStartable, IStopable
    {
        private const string LykkeExchangeName = "lykke";
        private const string Usd = "USD";
        
        private readonly object _lockArbitrages = new object();
        private readonly List<LykkeArbitrageRow> _arbitrages;
        private readonly TimerTrigger _trigger;
        private readonly IOrderBooksService _orderBooksService;
        private readonly ILog _log;

        public LykkeArbitrageDetectorService(IOrderBooksService orderBooksService, ISettingsService settingsService, ILogFactory logFactory)
        {
            _arbitrages = new List<LykkeArbitrageRow>();
            _orderBooksService = orderBooksService;
            _log = logFactory.CreateLog(this);

            var settings = settingsService.GetAsync().GetAwaiter().GetResult();
            var executionInterval = settings.LykkeArbitragesExecutionInterval;

            _trigger = new TimerTrigger(nameof(LykkeArbitrageDetectorService), executionInterval, logFactory, Execute);
        }

        public IEnumerable<LykkeArbitrageRow> GetArbitrages(string target, string source, ArbitrageProperty property = default, decimal minValue = 0)
        {
            IEnumerable<LykkeArbitrageRow> arbitrages;
            lock (_lockArbitrages)
            {
                arbitrages = _arbitrages.ToList();
            }

            // Filter by minValue
            arbitrages = GetFilterdByProperty(arbitrages, minValue, property);

            var result = new List<LykkeArbitrageRow>();

            // Filter by target
            if (!string.IsNullOrWhiteSpace(target))
                arbitrages = arbitrages.Where(x => x.Target.Name.Equals(target, StringComparison.OrdinalIgnoreCase));

            var groupedByTarget = arbitrages.GroupBy(x => x.Target);
            foreach (var targetPairArbitrages in groupedByTarget)
            {
                var targetArbitrages = targetPairArbitrages.ToList();

                // No target
                if (string.IsNullOrWhiteSpace(target))
                {
                    // Best arbitrage for each target
                    var bestByProperty = GetBestByProperty(targetArbitrages, property);
                    bestByProperty.SourcesCount = targetArbitrages.Count;
                    result.Add(bestByProperty);
                }
                // Target selected
                else
                {
                    // No source selected
                    if (string.IsNullOrWhiteSpace(source))
                    {
                        // Group by source
                        var groupedBySource = targetArbitrages.GroupBy(x => x.Source);

                        foreach (var group in groupedBySource)
                        {
                            // Best arbitrage by target and source
                            var targetGrouped = group.ToList();
                            var bestByProperty = GetBestByProperty(targetGrouped, property);
                            bestByProperty.SynthsCount = targetGrouped.Count;
                            result.Add(bestByProperty);
                        }
                    }
                    // Source selected
                    else
                    {
                        // Filter by source
                        targetArbitrages = targetArbitrages.Where(x => x.Source.Name.Equals(source, StringComparison.OrdinalIgnoreCase)).ToList();

                        var groupedBySource = targetArbitrages.GroupBy(x => x.Source);
                        foreach (var baseSourcePairsArbitrages in groupedBySource)
                        {
                            var baseSourceArbitrages = baseSourcePairsArbitrages.ToList();
                            result.AddRange(baseSourceArbitrages);
                        }
                    }
                }
            }

            result = GetOrderedByProperty(result, property).ToList();

            return result;
        }

        public async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
        {
            try
            {
                await Execute();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public async Task Execute()
        {
            var lykkeOrderBooks = _orderBooksService.GetAll()
                .Where(x => x.Source.Equals(LykkeExchangeName, StringComparison.InvariantCultureIgnoreCase)).ToList();
            var lykkeArbitrages = await GetArbitragesAsync(lykkeOrderBooks);
            RefreshArbitrages(lykkeArbitrages);
        }

        private Task<IReadOnlyList<LykkeArbitrageRow>> GetArbitragesAsync(IReadOnlyList<OrderBook> orderBooks)
        {
            orderBooks = orderBooks.Where(x => x.BestBid.HasValue || x.BestAsk.HasValue).OrderBy(x => x.AssetPair.Name).ToList();

            var result = new List<LykkeArbitrageRow>();

            var watch = Stopwatch.StartNew();

            var synthsCount = 0;
            // O( (n^2)/2 )
            for (var i = 0; i < orderBooks.Count; i++)
            {
                if (i == orderBooks.Count - 1)
                    break;

                var target = orderBooks.ElementAt(i);

                for (var j = i + 1; j < orderBooks.Count; j++)
                {
                    var source = orderBooks.ElementAt(j);

                    if (target.ToString() == source.ToString())
                        continue;

                    // Calculate all synthetic order books between a source order book and a target order book
                    var synthOrderBooks = SynthOrderBook.GetSynthsFromAll(target.AssetPair, source, orderBooks);
                    synthsCount += synthOrderBooks.Count;

                    // Compare each synthetic with current target asset pair
                    foreach (var synthOrderBook in synthOrderBooks)
                    {
                        decimal spread = 0;
                        decimal volume = 0;
                        decimal pnL = 0;
                        string targetSide = null;

                        // Bid side
                        if (target.BestBid?.Price > synthOrderBook.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(target.BestBid.Value.Price, synthOrderBook.BestAsk.Value.Price);
                            var volumePnL = Arbitrage.GetArbitrageVolumePnL(target.Bids, synthOrderBook.Asks);
                            Debug.Assert(volumePnL?.Volume != null);
                            Debug.Assert(volumePnL?.PnL != null);
                            targetSide = "Bid";
                            volume = volumePnL.Value.Volume;
                            pnL = volumePnL.Value.PnL;
                        }

                        // Ask side
                        if (synthOrderBook.BestBid?.Price > target.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(synthOrderBook.BestBid.Value.Price, target.BestAsk.Value.Price);
                            var volumePnL = Arbitrage.GetArbitrageVolumePnL(synthOrderBook.Bids, target.Asks);
                            Debug.Assert(volumePnL?.Volume != null);
                            Debug.Assert(volumePnL?.PnL != null);
                            targetSide = "Ask";
                            volume = volumePnL.Value.Volume;
                            pnL = volumePnL.Value.PnL;
                        }

                        if (string.IsNullOrWhiteSpace(targetSide)) // no arbitrage
                            continue;

                        var baseToUsdRate = Convert(target.AssetPair.Base, Usd, orderBooks);
                        var quoteToUsdRate = Convert(target.AssetPair.Quote, Usd, orderBooks);
                        var volumeInUsd = volume * baseToUsdRate;
                        volumeInUsd = volumeInUsd.HasValue ? Math.Round(volumeInUsd.Value) : (decimal?)null;
                        var pnLInUsd = pnL * quoteToUsdRate;
                        pnLInUsd = pnLInUsd.HasValue ? Math.Round(pnLInUsd.Value) : (decimal?)null;

                        var lykkeArbitrage = new LykkeArbitrageRow(target.AssetPair, source.AssetPair, spread, targetSide, synthOrderBook.ConversionPath,
                            volume, target.BestBid?.Price, target.BestAsk?.Price, synthOrderBook.BestBid?.Price, synthOrderBook.BestAsk?.Price, volumeInUsd,
                            pnL, pnLInUsd);
                        result.Add(lykkeArbitrage);
                    }
                }
            }

            watch.Stop();
            if (watch.ElapsedMilliseconds > 1000)
                _log.Info($"{watch.ElapsedMilliseconds} ms, {result.Count} arbitrages, {orderBooks.Count} order books, {synthsCount} synthetic order books.");

            return Task.FromResult(result.OrderBy(x => x.Target).ThenBy(x => x.Source).ToList() as IReadOnlyList<LykkeArbitrageRow>);
        }

        private void RefreshArbitrages(IEnumerable<LykkeArbitrageRow> lykkeArbitrages)
        {
            lock (_lockArbitrages)
            {
                _arbitrages.Clear();
                _arbitrages.AddRange(lykkeArbitrages);
            }
        }

        private decimal? Convert(string sourceAsset, string targetAsset, IReadOnlyList<OrderBook> orderBooks)
        {
            if (sourceAsset == targetAsset)
                return 1;

            var target = new AssetPair(sourceAsset, targetAsset, 8, 8);

            decimal? result = null;
            var synths1 = SynthOrderBook.GetSynthsFrom1(target, orderBooks, orderBooks);
            if (synths1.Any())
                result = synths1.First().BestAsk?.Price;

            return result;
        }

        private static LykkeArbitrageRow GetBestByProperty(IEnumerable<LykkeArbitrageRow> arbitrages, ArbitrageProperty property)
        {
            switch (property)
            {
                case ArbitrageProperty.Volume:
                    return arbitrages.MaxBy(x => x.VolumeInUsd);
                case ArbitrageProperty.Spread:
                    return arbitrages.MinBy(x => x.Spread);
                default:
                    return arbitrages.MaxBy(x => x.PnL);
            }
        }

        private static IEnumerable<LykkeArbitrageRow> GetOrderedByProperty(IEnumerable<LykkeArbitrageRow> arbitrages, ArbitrageProperty property)
        {
            switch (property)
            {
                case ArbitrageProperty.Volume:
                    return arbitrages.OrderByDescending(x => x.VolumeInUsd)
                                      .ThenByDescending(x => x.PnLInUsd)
                                      .ThenBy(x => x.Spread);
                case ArbitrageProperty.Spread:
                    return arbitrages.OrderBy(x => x.Spread)
                                      .ThenByDescending(x => x.PnLInUsd)
                                      .ThenByDescending(x => x.VolumeInUsd);
                default:
                    return arbitrages.OrderByDescending(x => x.PnLInUsd)
                                      .ThenByDescending(x => x.VolumeInUsd)
                                      .ThenBy(x => x.Spread);
            }
        }

        private static IEnumerable<LykkeArbitrageRow> GetFilterdByProperty(IEnumerable<LykkeArbitrageRow> arbitrages, decimal minValue, ArbitrageProperty property)
        {
            minValue = Math.Round(minValue, 2);

            // Filter by minValue
            if (minValue != 0)
                switch (property)
                {
                    case ArbitrageProperty.Volume: return arbitrages.Where(x => Math.Round(x.VolumeInUsd.Value, 2) >= minValue);
                    case ArbitrageProperty.Spread: return arbitrages.Where(x => Math.Round(Math.Abs(x.Spread), 2) >= minValue);
                }

            return arbitrages.Where(x => x.PnLInUsd >= minValue);
        }

        #region IStartable, IStopable

        public void Start()
        {
            _trigger.Start();
        }

        public void Stop()
        {
            _trigger.Stop();
        }

        public void Dispose()
        {
            _trigger?.Dispose();
        }

        #endregion
    }
}
