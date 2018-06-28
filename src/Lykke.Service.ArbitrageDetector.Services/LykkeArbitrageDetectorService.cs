using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class LykkeArbitrageDetectorService : TimerPeriod, ILykkeArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<AssetPairSource, OrderBook> _orderBooks;
        private readonly object _lockArbitrages = new object();
        private readonly List<LykkeArbitrageRow> _arbitrages;
        private ISettings _s;
        private readonly ILog _log;
        private readonly ISettingsRepository _settingsRepository;


        public LykkeArbitrageDetectorService(ILog log, IShutdownManager shutdownManager, ISettingsRepository settingsRepository)
            : base(100, log)
        {
            _orderBooks = new ConcurrentDictionary<AssetPairSource, OrderBook>();
            _arbitrages = new List<LykkeArbitrageRow>();

            _log = log;
            shutdownManager?.Register(this);
            _settingsRepository = settingsRepository;

            Task.Run(InitSettings).Wait();
        }

        private async Task InitSettings()
        {
            var dbSettings = await _settingsRepository.GetAsync();

            if (dbSettings == null)
            {
                dbSettings = Settings.Default;
                await _settingsRepository.InsertOrReplaceAsync(Settings.Default);
            }

            _s = dbSettings;
        }


        public void Process(OrderBook orderBook)
        {
            if (orderBook.AssetPair.IsEmpty())
            {
                var assets = new List<string>();
                assets.Add(_s.QuoteAsset);
                assets.AddRange(_s.BaseAssets);
                assets.AddRange(_s.IntermediateAssets);

                foreach (var asset in assets)
                {
                    if (!orderBook.AssetPairStr.Contains(asset))
                        continue;

                    orderBook.SetAssetPair(asset);
                    break;
                }
            }

            if (!orderBook.AssetPair.IsEmpty())
            {
                var key = new AssetPairSource(orderBook.Source, orderBook.AssetPair);
                _orderBooks.AddOrUpdate(key, orderBook);
            }
        }

        public override async Task Execute()
        {
            CalculateAndRefreshArbitrages();
        }

        private void CalculateAndRefreshArbitrages()
        {
            var lykkeArbitrages = GetArbitrages(_orderBooks.Values.ToList());

            lock (_lockArbitrages)
            {
                _arbitrages.Clear();
                _arbitrages.AddRange(lykkeArbitrages);
            }
        }

        private IReadOnlyCollection<LykkeArbitrageRow> GetArbitrages(IReadOnlyCollection<OrderBook> orderBooks)
        {
            var result = new List<LykkeArbitrageRow>();

            for (var i = 0; i < orderBooks.Count; i++)
            {
                if (i == orderBooks.Count - 1)
                    break;

                var basePair = orderBooks.ElementAt(i);
                for (var j = i + 1; j < orderBooks.Count; j++)
                {
                    var crossPair = orderBooks.ElementAt(j);

                    // Calculate all cross pairs between base order book and current order book
                    var crossRates = new Dictionary<AssetPairSource, CrossRate>();
                    var crossRateFrom1Or2Pairs = CrossRate.GetCrossRatesFrom1Or2Pairs(basePair.AssetPair, crossPair, orderBooks);
                    crossRates.AddRange(crossRateFrom1Or2Pairs);
                    var crossRateFrom3Pairs = CrossRate.GetCrossRatesFrom3Pairs(basePair.AssetPair, crossPair, orderBooks);
                    crossRates.AddRange(crossRateFrom3Pairs);

                    // Compare each cross pair with base pair
                    foreach (var crossRate in crossRates.Values)
                    {
                        var spread = decimal.MaxValue;
                        decimal volume = 0;
                        string baseSide = null;
                        
                        if (basePair.BestBid?.Price > crossRate.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(basePair.BestBid.Value.Price, crossRate.BestAsk.Value.Price);
                            volume = Arbitrage.GetArbitrageVolume(basePair.Bids, crossRate.Asks) ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            baseSide = "Bid";
                        }

                        if (crossRate.BestBid?.Price > basePair.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(crossRate.BestBid.Value.Price, basePair.BestAsk.Value.Price);
                            volume = Arbitrage.GetArbitrageVolume(crossRate.Bids, basePair.Asks) ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            baseSide = "Ask";
                        }

                        if (string.IsNullOrWhiteSpace(baseSide)) // no arbitrages
                            continue;

                        var lykkeArbitrage = new LykkeArbitrageRow(basePair.AssetPair, crossPair.AssetPair, spread, baseSide, crossRate.ConversionPath,
                            volume, basePair.BestBid?.Price, basePair.BestAsk?.Price, crossRate.BestBid?.Price, crossRate.BestAsk?.Price);
                        result.Add(lykkeArbitrage);
                    }
                }
            }

            return result.OrderBy(x => x.BaseAssetPair).ThenBy(x => x.CrossAssetPair).ToList();
        }

        public IEnumerable<LykkeArbitrageRow> GetArbitrages(string basePair, string crossPair)
        {
            var copy = new List<LykkeArbitrageRow>();
            lock (_lockArbitrages)
            {
                copy.AddRange(_arbitrages);
            }

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
                            bestBySpread.CrossRatesCount = crossPairGrouped.Count;

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
    }
}
