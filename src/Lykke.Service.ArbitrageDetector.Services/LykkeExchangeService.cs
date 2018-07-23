using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Client;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MoreLinq;
using AssetPair = Lykke.Service.ArbitrageDetector.Core.Domain.AssetPair;
using ILykkeAssetsService = Lykke.Service.Assets.Client.IAssetsService;
using LykkeAssetPair = Lykke.Service.Assets.Client.Models.AssetPair;
using OrderBook = Lykke.Service.ArbitrageDetector.Core.Domain.OrderBook;
using LykkeOrderBook = Lykke.Job.OrderBooksCacheProvider.Client.OrderBook;
using VolumePrice = Lykke.Service.ArbitrageDetector.Core.Domain.VolumePrice;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class LykkeExchangeService : ILykkeExchangeService, IStartable
    {
        private readonly ConcurrentDictionary<string, IList<Asset>> _assets = new ConcurrentDictionary<string, IList<Asset>>();
        private readonly ConcurrentDictionary<AssetPair, LykkeAssetPair> _assetPairs = new ConcurrentDictionary<AssetPair, LykkeAssetPair>();
        private readonly ConcurrentDictionary<AssetPair, (int Price, int Volume)> _accuracies = new ConcurrentDictionary<AssetPair, (int Price, int Volume)>();
        private readonly ConcurrentDictionary<AssetPair, OrderBook> _orderBooks = new ConcurrentDictionary<AssetPair, OrderBook>();

        public IArbitrageDetectorService ArbitrageDetectorService { get; set; }
        public ILykkeArbitrageDetectorService LykkeArbitrageDetectorService { get; set; }
        public ILykkeAssetsService AssetsService { get; set; }
        public IOrderBookProviderClient OrderBookProviderClient { get; set; }
        public ILog Log { get; set; }

        private void InitializeAssets()
        {
            var lykkeAssets = AssetsService.AssetGetAll();
            
            foreach (var lykkeAsset in lykkeAssets)
            {
                if (lykkeAsset != null)
                {
                    var name = lykkeAsset.Name;
                    var displayId = lykkeAsset.DisplayId;
                    var id = lykkeAsset.Id;

                    var shortestName = getShortestName(id, name, displayId);
                    if (!_assets.ContainsKey(shortestName))
                        _assets.Add(shortestName, new List<Asset> { lykkeAsset } );
                    else
                        _assets[shortestName].Add(lykkeAsset);
                }
            }

            Log.WriteInfoAsync(GetType().Name, nameof(InitializeAssets), $"Initialized {_assets.SelectMany(x => x.Value).Count()} of {lykkeAssets.Count} Lykke assets.");
        }

        private void InitializeAssetPairs()
        {
            var lykkeAssetPairs = AssetsService.AssetPairGetAll();

            foreach (var lykkeAssetPair in lykkeAssetPairs)
            {
                var allAssets = _assets.Values.SelectMany(x => x).ToList();
                var baseAsset = allAssets.SingleOrDefault(x => x.Id == lykkeAssetPair.BaseAssetId);
                var quoteAsset = allAssets.SingleOrDefault(x => x.Id == lykkeAssetPair.QuotingAssetId);

                if (baseAsset == null || quoteAsset == null)
                    continue;

                var baseName = getShortestName(baseAsset.Id, baseAsset.Name, baseAsset.DisplayId);
                var quoteName = getShortestName(quoteAsset.Id, quoteAsset.Name, quoteAsset.DisplayId);

                var assetPair = new AssetPair(baseName, quoteName);
                if (!_assetPairs.ContainsKey(assetPair))
                {
                    _assetPairs.Add(assetPair, lykkeAssetPair);
                    continue;
                }

                // If existed is shorter then skip current
                var existedLykkeAssetPair = _assetPairs[assetPair];
                if (existedLykkeAssetPair.Id.Length < lykkeAssetPair.Id.Length)
                    continue;

                // Current is shorter - remove existed and add current
                _assetPairs.Remove(assetPair);
                _assetPairs.Add(assetPair, lykkeAssetPair);
            }

            Log.WriteInfoAsync(GetType().Name, nameof(InitializeAssetPairs), $"Initialized {_assetPairs.Count} of {lykkeAssetPairs.Count} Lykke asset pairs.");
        }

        private void InitializeAccuracies()
        {
            foreach (var assetPair in _assetPairs)
            {
                var accuracy = (assetPair.Value.Accuracy, assetPair.Value.InvertedAccuracy);

                _accuracies.Add(assetPair.Key, accuracy);
            }

            Log.WriteInfoAsync(GetType().Name, nameof(InitializeAccuracies), $"Initialized {_accuracies.Count} accuracies.");
        }

        private void InitializeOrderBooks()
        {
            Log.WriteInfoAsync(GetType().Name, nameof(InitializeOrderBooks), $"Initializing Lykke order books...");

            foreach (var assetPairlykkeAssetPair in _assetPairs)
            {
                var assetPair = assetPairlykkeAssetPair.Key;
                var lykkeAssetPair = assetPairlykkeAssetPair.Value;

                LykkeOrderBook lykkeOrderBook;
                try
                {
                    lykkeOrderBook = OrderBookProviderClient.GetOrderBookAsync(lykkeAssetPair.Id).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    continue;
                }
                
                if (lykkeOrderBook == null)
                    continue;

                var orderBook = Convert(assetPair, lykkeOrderBook);

                _orderBooks.Add(assetPair, orderBook);
            }

            Log.WriteInfoAsync(GetType().Name, nameof(InitializeOrderBooks), $"Initialized {_orderBooks.Count} Lykke order books.");
        }

        private void InitializeServicesWithOrderBooks()
        {
            foreach (var orderBook in _orderBooks.Values)
            {
                ArbitrageDetectorService.Process(orderBook);
                LykkeArbitrageDetectorService.Process(orderBook);
            }
        }

        private OrderBook Convert(AssetPair assetPair, LykkeOrderBook lykkeOrderBook)
        {
            if (lykkeOrderBook == null)
                return null;

            var bids = new List<VolumePrice>();
            var asks = new List<VolumePrice>();

            foreach (var volumePrice in lykkeOrderBook.Prices)
            {
                if (volumePrice.Volume > 0)
                    bids.Add(new VolumePrice((decimal)volumePrice.Price, (decimal)volumePrice.Volume));
                else
                    asks.Add(new VolumePrice((decimal)volumePrice.Price, Math.Abs((decimal)volumePrice.Volume)));
            }

            var result = new OrderBook("lykke", assetPair.Name, bids, asks, lykkeOrderBook.Timestamp);
            result.AssetPair = assetPair;

            return result;
        }

        private void Initialize()
        {
            InitializeAssets();
            InitializeAssetPairs();
            InitializeAccuracies();
            InitializeOrderBooks();

            InitializeServicesWithOrderBooks();
        }

        private string getShortestName(string id, string name, string displayId)
        {
            var allNames = new List<string> { id, name, displayId };
            return allNames.Where(x => x != null).MinBy(x => x.Length);
        }

        public int InferBaseAndQuoteAssets(OrderBook orderBook)
        {
            if (orderBook == null)
                throw new ArgumentNullException(nameof(orderBook));

            var assetPairStr = orderBook.AssetPairStr;

            // The exact asset pair name
            var assetPair = _assetPairs.Keys.SingleOrDefault(x => string.Equals(x.Name, assetPairStr, StringComparison.OrdinalIgnoreCase));
            if (!assetPair.IsEmpty())
            {
                orderBook.AssetPair = assetPair;
                return 2;
            }

            AssetPair oneInfered;
            var assets = _assets.Keys;
            // Try to infer by assets
            foreach (var asset in assets)
            {
                if (assetPairStr.ToUpper().Contains(asset.ToUpper()))
                {
                    var otherAsset = assetPairStr.ToUpper().Replace(asset.ToUpper(), string.Empty);
                    var infered = 1;

                    if (assets.Any(x => string.Equals(x, otherAsset, StringComparison.OrdinalIgnoreCase)))
                        infered = 2;

                    var @base = assetPairStr.ToUpper().StartsWith(asset.ToUpper()) ? asset : otherAsset;
                    var quote = string.Equals(@base, asset, StringComparison.OrdinalIgnoreCase) ? otherAsset : asset;
                    assetPair = new AssetPair(@base, quote);

                    if (infered == 1)
                    {
                        oneInfered = assetPair;
                        continue; // still try to find both assets
                    }

                    // If found both assets then stop looking
                    orderBook.SetAssetPair(assetPair);
                    return 2;
                }
            }

            // If found only one asset then use it
            if (!oneInfered.IsEmpty())
            {
                orderBook.SetAssetPair(oneInfered);
                return 1;
            }

            return 0;
        }

        public (int Price, int Volume)? GetAccuracy(AssetPair assetPair)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentOutOfRangeException(nameof(assetPair));

            if (!_accuracies.ContainsKey(assetPair))
                return null;

            var foundAssetPair = _accuracies[assetPair];

            return foundAssetPair;
        }

        public void Start()
        {
            Initialize();
        }
    }
}
