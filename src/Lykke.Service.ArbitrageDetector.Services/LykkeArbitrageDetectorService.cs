using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class LykkeArbitrageDetectorService : TimerPeriod, ILykkeArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<AssetPairSource, OrderBook> _orderBooks;
        private readonly ConcurrentDictionary<AssetPair, LykkeArbitrageRow> _arbitrages;
        private ISettings _s;
        private readonly ILog _log;
        private readonly ISettingsRepository _settingsRepository;


        public LykkeArbitrageDetectorService(ILog log, IShutdownManager shutdownManager, ISettingsRepository settingsRepository)
            : base(100, log)
        {
            _orderBooks = new ConcurrentDictionary<AssetPairSource, OrderBook>();
            _arbitrages = new ConcurrentDictionary<AssetPair, LykkeArbitrageRow>();

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
            CalculateArbitrages();
        }

        private void CalculateArbitrages()
        {
            var result = new ConcurrentDictionary<AssetPair, LykkeArbitrageRow>();

            var orderBooks = _orderBooks.Values.ToList();

            for (var i = 0; i < orderBooks.Count; i++)
            {
                if (i == orderBooks.Count - 1)
                    break;

                var baseOrderBook = orderBooks[i];

                for (var j = i + 1; j < orderBooks.Count; j++)
                {
                    var currentOrderBook = orderBooks[j];

                    // Calculate all cross rates between base order book and current order book
                    var crossPairs = new Dictionary<AssetPairSource, CrossRate>();
                    var crossRateFrom1Or2Pairs = CrossRate.GetCrossRatesFrom1Or2Pairs(baseOrderBook.AssetPair, currentOrderBook, orderBooks);
                    crossPairs.AddRange(crossRateFrom1Or2Pairs);
                    var crossRateFrom3Pairs = CrossRate.GetCrossRatesFrom3Pairs(baseOrderBook.AssetPair, currentOrderBook, orderBooks);
                    crossPairs.AddRange(crossRateFrom3Pairs);

                    var minSpread = decimal.MaxValue;
                    CrossRate bestArbitrageBySpread = null;
                    string baseSide = null;
                    decimal? volume = null;

                    // Compare each cross rate with base order book
                    foreach (var crossRate in crossPairs.Values)
                    {
                        if (crossRate.BestBid.HasValue && baseOrderBook.BestAsk.HasValue && crossRate.BestBid.Value.Price > baseOrderBook.BestAsk.Value.Price)
                        {
                            var spread = Arbitrage.GetSpread(crossRate.BestBid.Value.Price, baseOrderBook.BestAsk.Value.Price);
                            if (spread < minSpread)
                            {
                                minSpread = spread;
                                bestArbitrageBySpread = crossRate;
                                baseSide = "ask";
                            }
                        }

                        if (crossRate.BestAsk.HasValue && baseOrderBook.BestBid.HasValue && baseOrderBook.BestBid.Value.Price > crossRate.BestAsk.Value.Price)
                        {
                            var spread = Arbitrage.GetSpread(baseOrderBook.BestBid.Value.Price, crossRate.BestAsk.Value.Price);
                            if (spread < minSpread)
                            {
                                minSpread = spread;
                                bestArbitrageBySpread = crossRate;
                                baseSide = "bid";
                            }
                        }
                    }

                    if (minSpread < 0)
                    {
                        if (baseSide == "ask")
                            volume = Arbitrage.GetArbitrageVolume(bestArbitrageBySpread.Bids, baseOrderBook.Asks);

                        if (baseSide == "bid")
                            volume = Arbitrage.GetArbitrageVolume(baseOrderBook.Bids, bestArbitrageBySpread.Asks);

                        result.AddOrUpdate(baseOrderBook.AssetPair, new LykkeArbitrageRow(baseOrderBook.AssetPair, bestArbitrageBySpread.AssetPair,
                            minSpread, baseSide, bestArbitrageBySpread.ConversionPath, volume.Value,
                            baseOrderBook.BestBid?.Price, baseOrderBook.BestAsk?.Price, bestArbitrageBySpread.BestBid?.Price, bestArbitrageBySpread.BestAsk?.Price));
                    }
                }
            }

            _arbitrages.Clear();
            _arbitrages.AddRange(result);
        }

        public IEnumerable<LykkeArbitrageRow> GetArbitrages()
        {
            if (!_arbitrages.Any())
                return new List<LykkeArbitrageRow>();

            return _arbitrages.Select(x => x.Value)
                .OrderBy(x => x.BaseAssetPair.Name)
                .ToList();
        }
    }
}
