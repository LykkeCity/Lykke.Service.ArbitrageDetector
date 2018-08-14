using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Core.Services.Infrastructure;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class LykkeArbitrageDetectorService : ILykkeArbitrageDetectorService, IStartable, IStopable
    {
        private static readonly TimeSpan DefaultInterval = new TimeSpan(0, 0, 0, 2);
        private const string LykkeExchangeName = "lykke";

        private readonly ConcurrentDictionary<AssetPair, OrderBook> _orderBooks;
        private readonly object _lockArbitrages = new object();
        private readonly List<LykkeArbitrageRow> _arbitrages;
        private readonly TimerTrigger _trigger;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly ILog _log;

        public LykkeArbitrageDetectorService(ILog log, IShutdownManager shutdownManager,
            ILykkeExchangeService lykkeExchangeService, IArbitrageDetectorService arbitrageDetectorService)
        {
            shutdownManager?.Register(this);

            _orderBooks = new ConcurrentDictionary<AssetPair, OrderBook>();
            _arbitrages = new List<LykkeArbitrageRow>();

            _arbitrageDetectorService = arbitrageDetectorService ?? throw new ArgumentNullException(nameof(arbitrageDetectorService));
            _lykkeExchangeService = lykkeExchangeService ?? throw new ArgumentNullException(nameof(lykkeExchangeService));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _trigger = new TimerTrigger(nameof(LykkeArbitrageDetectorService), DefaultInterval, log, Execute);
        }

        public void Process(OrderBook orderBook)
        {
            var isLykkeExchange = string.Equals(orderBook.Source, LykkeExchangeName, StringComparison.OrdinalIgnoreCase);
            if (isLykkeExchange && _lykkeExchangeService.InferBaseAndQuoteAssets(orderBook) > 0)
            {
                _orderBooks.AddOrUpdate(orderBook.AssetPair, orderBook);
            }
        }

        public async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
        {
            try
            {
                await Execute();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(Execute), ex);
            }
        }

        public async Task Execute()
        {
            var lykkeArbitrages = await GetArbitrages(_orderBooks.Values.ToList());
            RefreshArbitrages(lykkeArbitrages);
        }

        private async Task<IReadOnlyCollection<LykkeArbitrageRow>> GetArbitrages(IReadOnlyCollection<OrderBook> orderBooks)
        {
            orderBooks = orderBooks.Where(x => x.BestBid.HasValue || x.BestAsk.HasValue).ToList();

            var result = new List<LykkeArbitrageRow>();

            var watch = Stopwatch.StartNew();

            var minSpread = Settings().MinSpread;
            var synthMaxDepth = Settings().SynthMaxDepth;

            var synthsCount = 0;
            var totalItarations = 0;
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

                    // Calculate all synthetic order books between source order book and target order book
                    var synthOrderBooks = SynthOrderBook.GetSynthsFromAll(target.AssetPair, source, orderBooks, /*synthMaxDepth*/int.MaxValue);
                    synthsCount += synthOrderBooks.Count;

                    // Compare each synthetic with target
                    foreach (var synthOrderBook in synthOrderBooks.Values)
                    {
                        totalItarations++;

                        decimal spread = 0;
                        decimal volume = 0;
                        decimal pnL = 0;
                        string targetSide = null;

                        if (target.BestBid?.Price > synthOrderBook.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(target.BestBid.Value.Price, synthOrderBook.BestAsk.Value.Price);
                            if (minSpread < 0 && spread < minSpread)
                                continue;
                            var volumePnL = Arbitrage.GetArbitrageVolumePnL(target.Bids, synthOrderBook.Asks);
                            volume = volumePnL?.Volume ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            pnL = volumePnL?.PnL ?? throw new InvalidOperationException("Every arbitrage must have PnL");
                            targetSide = "Bid";
                        }

                        if (synthOrderBook.BestBid?.Price > target.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(synthOrderBook.BestBid.Value.Price, target.BestAsk.Value.Price);
                            if (minSpread < 0 && spread < minSpread)
                                continue;
                            var volumePnL = Arbitrage.GetArbitrageVolumePnL(synthOrderBook.Bids, target.Asks);
                            volume = volumePnL?.Volume ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            pnL = volumePnL?.PnL ?? throw new InvalidOperationException("Every arbitrage must have PnL");
                            targetSide = "Ask";
                        }

                        if (string.IsNullOrWhiteSpace(targetSide)) // no arbitrages
                            continue;

                        var baseToUsdRate = Convert(target.AssetPair.Base, "USD", _orderBooks.Values.ToList());
                        var quoteToUsdRate = Convert(target.AssetPair.Quote, "USD", _orderBooks.Values.ToList());
                        var volumeInUsd = volume * baseToUsdRate;
                        var pnLInUsd = pnL * quoteToUsdRate;

                        var lykkeArbitrage = new LykkeArbitrageRow(target.AssetPair, source.AssetPair, spread, targetSide, synthOrderBook.ConversionPath,
                            volume, target.BestBid?.Price, target.BestAsk?.Price, synthOrderBook.BestBid?.Price, synthOrderBook.BestAsk?.Price, volumeInUsd,
                            pnL, pnLInUsd);
                        result.Add(lykkeArbitrage);
                    }
                }
            }

            watch.Stop();
            if (watch.ElapsedMilliseconds > 1000)
                await _log.WriteInfoAsync(GetType().Name, nameof(GetArbitrages), $"{watch.ElapsedMilliseconds} ms, {result.Count} arbitrages, {orderBooks.Count} order books, {synthsCount} synthetic order books created, {totalItarations} iterations.");

            return result.OrderBy(x => x.Target).ThenBy(x => x.Source).ToList();
        }

        private void RefreshArbitrages(IEnumerable<LykkeArbitrageRow> lykkeArbitrages)
        {
            lock (_lockArbitrages)
            {
                _arbitrages.Clear();
                _arbitrages.AddRange(lykkeArbitrages);
            }
        }

        private decimal? Convert(string sourceAsset, string targetAsset, IReadOnlyCollection<OrderBook> orderBooks)
        {
            if (sourceAsset == targetAsset)
                return 1;

            var target = new AssetPair(sourceAsset, targetAsset);

            decimal? result = null;
            var synths1 = SynthOrderBook.GetSynthsFrom1(target, orderBooks);
            if (synths1.Any())
                result = GetMedianAskPrice(synths1.Values.ToList());

            if (result.HasValue)
                return result;

            var synths2 = SynthOrderBook.GetSynthsFrom2(target, orderBooks);
            if (synths2.Any())
                result = GetMedianAskPrice(synths2.Values.ToList());

            if (result.HasValue)
                return result;

            var synths3 = SynthOrderBook.GetSynthsFrom3(target, orderBooks);
            if (synths3.Any())
                result = GetMedianAskPrice(synths3.Values.ToList());

            return result;
        }

        private decimal? GetMedianAskPrice(IEnumerable<SynthOrderBook> synths)
        {
            decimal? result = null;

            var bestAsks = synths.Where(x => x.BestAsk.HasValue).Select(x => x.BestAsk.Value.Price).OrderBy(x => x).ToList();

            if (bestAsks.Any())
                result = bestAsks.ElementAt(bestAsks.Count / 2);

            return result;
        }

        private Settings Settings()
        {
            return _arbitrageDetectorService.GetSettings();
        }


        public IEnumerable<LykkeArbitrageRow> GetArbitrages(string target, string source, ArbitrageProperty property = default, decimal minValue = 0)
        {
            var copy = new List<LykkeArbitrageRow>();
            lock (_lockArbitrages)
            {
                copy.AddRange(_arbitrages);
            }

            // Filter by minValue
            if (minValue != 0)
                switch (property)
                {
                    case ArbitrageProperty.Volume:
                        copy = copy.Where(x => x.VolumeInUsd >= minValue).ToList();
                        break;
                    case ArbitrageProperty.Spread:
                        copy = copy.Where(x => Math.Abs(x.Spread) >= minValue).ToList();
                        break;
                    default:
                        copy = copy.Where(x => x.PnLInUsd >= minValue).ToList();
                        break;
                }

            var result = new List<LykkeArbitrageRow>();

            // Filter by target
            if (!string.IsNullOrWhiteSpace(target))
                copy = copy.Where(x => x.Target.Name.Equals(target, StringComparison.OrdinalIgnoreCase)).ToList();

            var groupedByTarget = copy.GroupBy(x => x.Target);
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
