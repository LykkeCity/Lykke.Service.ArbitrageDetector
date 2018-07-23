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

        private readonly ConcurrentDictionary<AssetPair, OrderBook> _orderBooks;
        private readonly object _lockArbitrages = new object();
        private readonly List<LykkeArbitrageRow> _arbitrages;
        private readonly TimerTrigger _trigger;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly ILog _log;
        
        public LykkeArbitrageDetectorService(ILog log, IShutdownManager shutdownManager, ILykkeExchangeService lykkeExchangeService)
        {
            shutdownManager?.Register(this);

            _orderBooks = new ConcurrentDictionary<AssetPair, OrderBook>();
            _arbitrages = new List<LykkeArbitrageRow>();

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

            // O( (n^2) )
            for (var i = 0; i < orderBooks.Count; i++)
            {
                if (i == orderBooks.Count - 1)
                    break;

                var target = orderBooks.ElementAt(i);

                // Can be "var j = i + 1" to decrease uneccessary (dublicated, swaped base and cross) comparisons
                for (var j = 0; j < orderBooks.Count; j++)
                {
                    var source = orderBooks.ElementAt(j);

                    // Calculate all synthetic order books between base order book and current order book
                    var synthOrderBooks = SynthOrderBook.GetSynthsFromAll(target.AssetPair, source, orderBooks);

                    // Compare each cross pair with base pair
                    foreach (var synthOrderBook in synthOrderBooks.Values)
                    {
                        decimal spread = 0;
                        decimal volume = 0;
                        decimal pnL = 0;
                        string targetSide = null;

                        if (target.BestBid?.Price > synthOrderBook.BestAsk?.Price)
                        {
                            var volumePnL = Arbitrage.GetArbitrageVolumePnL(target.Bids, synthOrderBook.Asks);
                            spread = Arbitrage.GetSpread(target.BestBid.Value.Price, synthOrderBook.BestAsk.Value.Price);
                            volume = volumePnL?.Volume ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            pnL = volumePnL?.PnL ?? throw new InvalidOperationException("Every arbitrage must have PnL");
                            targetSide = "Bid";
                        }

                        if (synthOrderBook.BestBid?.Price > target.BestAsk?.Price)
                        {
                            var volumePnL = Arbitrage.GetArbitrageVolumePnL(synthOrderBook.Bids, target.Asks);
                            spread = Arbitrage.GetSpread(synthOrderBook.BestBid.Value.Price, target.BestAsk.Value.Price);
                            volume = volumePnL?.Volume ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            pnL = volumePnL?.PnL ?? throw new InvalidOperationException("Every arbitrage must have PnL");
                            targetSide = "Ask";
                        }

                        if (string.IsNullOrWhiteSpace(targetSide)) // no arbitrages
                            continue;

                        var baseToUsd = Convert(target.AssetPair.Base, "USD", _orderBooks.Values.ToList());
                        var quoteToUsd = Convert(target.AssetPair.Quote, "USD", _orderBooks.Values.ToList());
                        var volumeInUsd = volume * baseToUsd;
                        var pnLInUsd = pnL * quoteToUsd;

                        var lykkeArbitrage = new LykkeArbitrageRow(target.AssetPair, source.AssetPair, spread, targetSide, synthOrderBook.ConversionPath,
                            volume, target.BestBid?.Price, target.BestAsk?.Price, synthOrderBook.BestBid?.Price, synthOrderBook.BestAsk?.Price, volumeInUsd, pnL, pnLInUsd);
                        result.Add(lykkeArbitrage);
                    }
                }
            }

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
                copy = copy.Where(x => string.Equals(x.Target.Name, basePair, StringComparison.OrdinalIgnoreCase)).ToList();

            var groupedByBasePair = copy.GroupBy(x => x.Target);
            foreach (var basePairArbitrages in groupedByBasePair)
            {
                var baseArbitrages = basePairArbitrages.ToList();

                // No base pair
                if (string.IsNullOrWhiteSpace(basePair))
                {
                    // Best cross pair for each base pair
                    var bestBySpread = baseArbitrages.MinBy(x => x.Spread);
                    bestBySpread.SourcesCount = baseArbitrages.Count;

                    result.Add(bestBySpread);
                }
                // Base pair selected
                else
                {
                    // No cross pair
                    if (string.IsNullOrWhiteSpace(crossPair))
                    {
                        // Group by cross pair
                        var groupedByCrossPair = baseArbitrages.GroupBy(x => x.Source);

                        foreach (var group in groupedByCrossPair)
                        {
                            var crossPairGrouped = group.ToList();
                            var bestBySpread = crossPairGrouped.MinBy(x => x.Spread);
                            bestBySpread.SynthsCount = crossPairGrouped.Count;

                            result.Add(bestBySpread);
                        }
                    }
                    // Cross pair selected
                    else
                    {
                        // Filter by cross pair
                        baseArbitrages = baseArbitrages.Where(x => string.Equals(x.Source.Name, crossPair, StringComparison.OrdinalIgnoreCase)).ToList();

                        var groupedByCrossPair = baseArbitrages.GroupBy(x => x.Source);
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
