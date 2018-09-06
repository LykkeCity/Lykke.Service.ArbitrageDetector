using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Client;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MoreLinq;
using AssetPair = Lykke.Service.ArbitrageDetector.Core.Domain.AssetPair;
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

        private readonly IAssetsService _assetsService;
        private readonly IOrderBookProviderClient _orderBookProviderClient;
        private readonly ILog _log;

        public LykkeExchangeService(IAssetsService assetsService, IOrderBookProviderClient orderBookProviderClient, ILogFactory logFactory)
        {
            _assetsService = assetsService;
            _orderBookProviderClient = orderBookProviderClient;
            _log = logFactory.CreateLog(this);
        }

        private void InitializeAssets()
        {
            var lykkeAssets = _assetsService.AssetGetAll();
            
            foreach (var lykkeAsset in lykkeAssets)
            {
                if (lykkeAsset != null)
                {
                    var name = lykkeAsset.Name;
                    var displayId = lykkeAsset.DisplayId;
                    var id = lykkeAsset.Id;

                    var shortestName = GetShortestName(id, name, displayId);
                    if (!_assets.ContainsKey(shortestName))
                        _assets[shortestName] = new List<Asset> { lykkeAsset };
                    else
                        _assets[shortestName].Add(lykkeAsset);
                }
            }

            _log.Info($"Initialized {_assets.SelectMany(x => x.Value).Count()} of {lykkeAssets.Count} Lykke assets.");
        }

        private void InitializeAssetPairs()
        {
            var lykkeAssetPairs = _assetsService.AssetPairGetAll();

            foreach (var lykkeAssetPair in lykkeAssetPairs)
            {
                var allAssets = _assets.Values.SelectMany(x => x).ToList();
                var baseAsset = allAssets.SingleOrDefault(x => x.Id == lykkeAssetPair.BaseAssetId);
                var quoteAsset = allAssets.SingleOrDefault(x => x.Id == lykkeAssetPair.QuotingAssetId);

                if (baseAsset == null || quoteAsset == null)
                    continue;

                var baseName = GetShortestName(baseAsset.Id, baseAsset.Name, baseAsset.DisplayId);
                var quoteName = GetShortestName(quoteAsset.Id, quoteAsset.Name, quoteAsset.DisplayId);

                var assetPair = new AssetPair(baseName, quoteName);
                if (!_assetPairs.ContainsKey(assetPair))
                {
                    _assetPairs[assetPair] = lykkeAssetPair;
                    continue;
                }

                // If existed is shorter then skip current
                var existedLykkeAssetPair = _assetPairs[assetPair];
                if (existedLykkeAssetPair.Id.Length < lykkeAssetPair.Id.Length)
                    continue;

                // Current is shorter - remove existed and add current
                _assetPairs.Remove(assetPair);
                _assetPairs[assetPair] = lykkeAssetPair;
            }

            _log.Info($"Initialized {_assetPairs.Count} of {lykkeAssetPairs.Count} Lykke asset pairs.");
        }

        private void InitializeAccuracies()
        {
            foreach (var assetPair in _assetPairs)
            {
                var accuracy = (assetPair.Value.Accuracy, assetPair.Value.InvertedAccuracy);

                _accuracies[assetPair.Key] = accuracy;
            }

            _log.Info($"Initialized {_accuracies.Count} accuracies.");
        }

        private async Task InitializeOrderBooks()
        {
            _log.Info($"Initializing Lykke order books...");

            foreach (var assetPairlykkeAssetPair in _assetPairs)
            {
                var assetPair = assetPairlykkeAssetPair.Key;
                var lykkeAssetPair = assetPairlykkeAssetPair.Value;

                LykkeOrderBook lykkeOrderBook;
                try
                {
                    lykkeOrderBook = await _orderBookProviderClient.GetOrderBookAsync(lykkeAssetPair.Id);
                }
                catch (Exception)
                {
                    // Some order books can't be found by asset pair id
                    continue;
                }
                
                if (lykkeOrderBook == null)
                    continue;

                var orderBook = Convert(assetPair, lykkeOrderBook);

                _orderBooks[assetPair] = orderBook;
            }

            _log.Info($"Initialized {_orderBooks.Count} Lykke order books.");
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

            var result = new OrderBook("lykke", assetPair, bids, asks, lykkeOrderBook.Timestamp);

            return result;
        }

        private void Initialize()
        {
            InitializeAssets();
            InitializeAssetPairs();
            InitializeAccuracies();

            Task.Run(async () =>
                {
                    await InitializeOrderBooks();
                })
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        _log.Error(t.Exception, "Can't initialize order books from cache provider.");
                });
        }

        private string GetShortestName(string id, string name, string displayId)
        {
            var allNames = new List<string> { id, name, displayId };
            return allNames.Where(x => x != null).MinBy(x => x.Length);
        }

        public AssetPair? InferBaseAndQuoteAssets(string assetPairStr)
        {
            if (string.IsNullOrWhiteSpace(assetPairStr))
                throw new ArgumentNullException(nameof(assetPairStr));

            // The exact asset pair name
            var assetPair = _assetPairs.Keys.SingleOrDefault(x => string.Equals(x.Name, assetPairStr, StringComparison.OrdinalIgnoreCase));
            if (!assetPair.IsEmpty())
            {
                return assetPair;
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

                    if (string.IsNullOrWhiteSpace(@base) || string.IsNullOrWhiteSpace(quote))
                    {
                        _log.Info($"Strange situation with asset inference - assetPairStr: {assetPairStr}, found base: '{@base}', found quote: '{quote}', assets: {string.Join(", ", assets.Select(x => $"'{x}'"))}");
                        continue;
                    }

                    assetPair = new AssetPair(@base, quote);

                    if (infered == 1)
                    {
                        oneInfered = assetPair;
                        continue; // still try to find both assets
                    }

                    // If found both assets then stop looking
                    return assetPair;
                }
            }

            // If found only one asset then use it
            if (!oneInfered.IsEmpty())
            {
                return oneInfered;
            }

            return null;
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
