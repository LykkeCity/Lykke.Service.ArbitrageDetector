using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private readonly ConcurrentDictionary<AssetPairSource, OrderBook> _orderBooks;
        private readonly object _lockArbitrages = new object();
        private readonly List<LykkeArbitrageRow> _arbitrages;
        private readonly TimerTrigger _trigger;
        private readonly IAssetsService _assetsService;
        private readonly ILog _log;
        
        public LykkeArbitrageDetectorService(ILog log, IShutdownManager shutdownManager, IAssetsService assetsService)
        {
            shutdownManager?.Register(this);

            _orderBooks = new ConcurrentDictionary<AssetPairSource, OrderBook>();
            _arbitrages = new List<LykkeArbitrageRow>();

            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _trigger = new TimerTrigger(nameof(LykkeArbitrageDetectorService), DefaultInterval, log, Execute);
        }

        public void Process(OrderBook orderBook)
        {
            var isLykkeExchange = string.Equals(orderBook.Source, LykkeExchangeName, StringComparison.OrdinalIgnoreCase);
            if (isLykkeExchange && _assetsService.InferBaseAndQuoteAssets(orderBook) > 0)
            {
                var key = new AssetPairSource(orderBook.Source, orderBook.AssetPair);
                _orderBooks.AddOrUpdate(key, orderBook);
            }
        }

        public async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
        {
            try
            {
                Execute();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(Execute), ex);
            }
        }

        public void Execute()
        {
            var lykkeArbitrages = GetArbitrages(_orderBooks.Values.ToList());
            RefreshArbitrages(lykkeArbitrages);
        }

        private IReadOnlyCollection<LykkeArbitrageRow> GetArbitrages(IReadOnlyCollection<OrderBook> orderBooks)
        {
            var result = new List<LykkeArbitrageRow>();

            // O( (n^2)/2 )
            for (var i = 0; i < orderBooks.Count; i++)
            {
                if (i == orderBooks.Count - 1)
                    break;

                var basePair = orderBooks.ElementAt(i);
                for (var j = i + 1; j < orderBooks.Count; j++)
                {
                    var crossPair = orderBooks.ElementAt(j);

                    // Calculate all synthetic order books between base order book and current order book
                    var synthOrderBooks = SynthOrderBook.GetSynthsFromAll(basePair.AssetPair, crossPair, orderBooks);

                    // Compare each cross pair with base pair
                    foreach (var synthOrderBook in synthOrderBooks.Values)
                    {
                        var spread = decimal.MaxValue;
                        decimal volume = 0;
                        string baseSide = null;

                        if (basePair.BestBid?.Price > synthOrderBook.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(basePair.BestBid.Value.Price, synthOrderBook.BestAsk.Value.Price);
                            volume = Arbitrage.GetArbitrageVolume(basePair.Bids, synthOrderBook.Asks) ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            baseSide = "Bid";
                        }

                        if (synthOrderBook.BestBid?.Price > basePair.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(synthOrderBook.BestBid.Value.Price, basePair.BestAsk.Value.Price);
                            volume = Arbitrage.GetArbitrageVolume(synthOrderBook.Bids, basePair.Asks) ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            baseSide = "Ask";
                        }

                        if (string.IsNullOrWhiteSpace(baseSide)) // no arbitrages
                            continue;

                        var baseToUsd = Convert(basePair.AssetPair.Base, "USD", _orderBooks.Values.ToList());
                        var volumeInUsd = volume * baseToUsd;

                        var lykkeArbitrage = new LykkeArbitrageRow(basePair.AssetPair, crossPair.AssetPair, spread, baseSide, synthOrderBook.ConversionPath,
                            volume, basePair.BestBid?.Price, basePair.BestAsk?.Price, synthOrderBook.BestBid?.Price, synthOrderBook.BestAsk?.Price, volumeInUsd);
                        result.Add(lykkeArbitrage);
                    }
                }
            }

            return result.OrderBy(x => x.BaseAssetPair).ThenBy(x => x.CrossAssetPair).ToList();
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
            var targetAssetPair = new AssetPair(sourceAsset, targetAsset);

            var synths1 = SynthOrderBook.GetSynthsFrom1(targetAssetPair, orderBooks);
            if (synths1.Any())
                return GetMedianAskPrice(synths1.Values);

            var synths2 = SynthOrderBook.GetSynthsFrom2(targetAssetPair, orderBooks);
            if (synths2.Any())
                return GetMedianAskPrice(synths2.Values);

            var synths3 = SynthOrderBook.GetSynthsFrom3(targetAssetPair, orderBooks);
            if (synths3.Any())
                return GetMedianAskPrice(synths3.Values);

            return null;
        }

        private decimal? GetMedianAskPrice(IEnumerable<SynthOrderBook> synths)
        {
            decimal? result = null;

            var bestAsks = synths.Where(x => x.BestAsk.HasValue).Select(x => x.BestAsk.Value.Price).OrderBy(x => x).ToList();

            if (bestAsks.Any())
                result = bestAsks.ElementAt(bestAsks.Count / 2);

            return result;
        }


        public IEnumerable<LykkeArbitrageRow> GetArbitrages(string basePair, string crossPair, decimal minVolumeInUsd = 0)
        {
            var copy = new List<LykkeArbitrageRow>();
            lock (_lockArbitrages)
            {
                copy.AddRange(_arbitrages);
            }

            // Filter by minVolumeInUsd
            copy = copy.Where(x => x.VolumeInUsd > minVolumeInUsd).ToList();

            var result = new List<LykkeArbitrageRow>();

            // Filter by basePair
            if (!string.IsNullOrWhiteSpace(basePair))
                copy = copy.Where(x => string.Equals(x.BaseAssetPair.Name, basePair, StringComparison.OrdinalIgnoreCase)).ToList();

            var groupedByBasePair = copy.GroupBy(x => x.BaseAssetPair);
            foreach (var basePairArbitrages in groupedByBasePair)
            {
                var baseArbitrages = basePairArbitrages.ToList();

                // No base pair
                if (string.IsNullOrWhiteSpace(basePair))
                {
                    // Best cross pair for each base pair
                    var bestBySpread = baseArbitrages.MinBy(x => x.Spread);
                    bestBySpread.CrossPairsCount = baseArbitrages.Count;

                    result.Add(bestBySpread);
                }
                // Base pair selected
                else
                {
                    // No cross pair
                    if (string.IsNullOrWhiteSpace(crossPair))
                    {
                        // Group by cross pair
                        var groupedByCrossPair = baseArbitrages.GroupBy(x => x.CrossAssetPair);

                        foreach (var group in groupedByCrossPair)
                        {
                            var crossPairGrouped = group.ToList();
                            var bestBySpread = crossPairGrouped.MinBy(x => x.Spread);
                            bestBySpread.SynthOrderBooksCount = crossPairGrouped.Count;

                            result.Add(bestBySpread);
                        }
                    }
                    // Cross pair selected
                    else
                    {
                        // Filter by cross pair
                        baseArbitrages = baseArbitrages.Where(x => string.Equals(x.CrossAssetPair.Name, crossPair, StringComparison.OrdinalIgnoreCase)).ToList();

                        var groupedByCrossPair = baseArbitrages.GroupBy(x => x.CrossAssetPair);
                        foreach (var baseCrossPairsArbitrages in groupedByCrossPair)
                        {
                            var baseCrossArbitrages = baseCrossPairsArbitrages.ToList();

                            result.AddRange(baseCrossArbitrages);
                        }
                    }
                }
            }

            return result;
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
