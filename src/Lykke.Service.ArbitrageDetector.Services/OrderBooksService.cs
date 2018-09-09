using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Client;
using Lykke.Service.ArbitrageDetector.Core.Handlers;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.Assets.Client;
using MoreLinq;
using AssetPair = Lykke.Service.ArbitrageDetector.Core.Domain.AssetPair;
using OrderBook = Lykke.Service.ArbitrageDetector.Core.Domain.OrderBook;
using CacheProviderOrderBook = Lykke.Job.OrderBooksCacheProvider.Client.OrderBook;
using VolumePrice = Lykke.Service.ArbitrageDetector.Core.Domain.VolumePrice;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class OrderBooksService : IOrderBooksService, IOrderBookHandler, IStartable
    {
        private const string LykkeExchangeName = "lykke";

        private readonly IAssetsService _assetsService;
        private readonly IOrderBookProviderClient _orderBookProviderClient;
        private readonly ILog _log;

        private readonly ConcurrentDictionary<string, string> _lykkeAssets = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, AssetPair> _lykkeAssetIdsAssetPairs = new ConcurrentDictionary<string, AssetPair>();
        private readonly ConcurrentDictionary<string, AssetPair> _lykkeAssetNamesAssetPairs = new ConcurrentDictionary<string, AssetPair>();
        private readonly ConcurrentDictionary<string, OrderBook> _allOrderBooks = new ConcurrentDictionary<string, OrderBook>();

        private readonly object _syncLykke = new object();
        private readonly Dictionary<string, OrderBook> _dirtyLykkeOrderBooks = new Dictionary<string, OrderBook>();

        public OrderBooksService(IOrderBookProviderClient orderBookProviderClient,
            IAssetsService assetsService, ILogFactory logFactory)
        {
            _orderBookProviderClient = orderBookProviderClient;
            _assetsService = assetsService;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            Initialize();
        }

        public IReadOnlyList<OrderBook> GetAll()
        {
            var result = _allOrderBooks.Values.OrderBy(x => x.AssetPair.Name).ToList();

            return result;
        }

        public OrderBook Get(string assetPairId)
        {
            return GetAll().SingleOrDefault(x => x.AssetPair.Name == assetPairId);
        }

        public IReadOnlyList<OrderBook> GetOrderBooks(string exchange, string assetPair)
        {
            var result = GetAll();

            if (!result.Any())
                return new List<OrderBook>();

            if (!string.IsNullOrWhiteSpace(exchange))
                result = result.Where(x => x.Source.ToUpper().Trim().Contains(exchange.ToUpper().Trim())).ToList();

            if (!string.IsNullOrWhiteSpace(assetPair))
                result = result.Where(x => x.AssetPair.Name.ToUpper().Trim().Contains(assetPair.ToUpper().Trim())).ToList();

            return result.OrderByDescending(x => x.AssetPair.Name).ToList();
        }

        public OrderBook GetOrderBook(string exchange, string assetPair)
        {
            if (string.IsNullOrWhiteSpace(exchange))
                throw new ArgumentException($"{nameof(exchange)} must be set.");

            if (string.IsNullOrWhiteSpace(assetPair))
                throw new ArgumentException($"{nameof(assetPair)} must be set.");

            var orderBooks = GetAll();

            if (!orderBooks.Any())
                return null;

            var result = orderBooks.SingleOrDefault(x => x.Source.Equals(exchange, StringComparison.OrdinalIgnoreCase)
                                                        && x.AssetPair.Name.Equals(assetPair, StringComparison.OrdinalIgnoreCase));

            return result;
        }

        public async Task HandleAsync(OrderBook orderBook)
        {
            // Lykke exchage
            if (string.Equals(orderBook.Source, LykkeExchangeName, StringComparison.InvariantCultureIgnoreCase))
            {
                await HandleLykkeOrderBook(orderBook);
            }
            // Others exchanges
            else
            {
                var key = GetKeyForOrderBooks(orderBook);
                _allOrderBooks[key] = orderBook;
            }
        }

        public AssetPair InferBaseAndQuoteAssets(string assetPairStr)
        {
            if (string.IsNullOrWhiteSpace(assetPairStr))
                throw new ArgumentNullException(nameof(assetPairStr));

            // The exact asset pair name
            _lykkeAssetNamesAssetPairs.TryGetValue(assetPairStr, out var assetPair);
            if (assetPair != null)
            {
                return assetPair;
            }

            AssetPair oneInfered = null;
            var assets = _lykkeAssets.Values.ToList();
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

                    assetPair = new AssetPair(@base, quote, 8, 8);

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
            return oneInfered;
        }

        private Task HandleLykkeOrderBook(OrderBook orderBook)
        {
            var assetPairName = orderBook.AssetPair.Name;
            if (!_lykkeAssetNamesAssetPairs.ContainsKey(assetPairName))
                return Task.CompletedTask;

            lock (_syncLykke)
            {
                if (!_dirtyLykkeOrderBooks.ContainsKey(assetPairName))
                {
                    _dirtyLykkeOrderBooks.Add(assetPairName, orderBook);
                }
                else
                {
                    // Update half even if it already exists
                    var dirtyOrderBook = _dirtyLykkeOrderBooks[assetPairName];

                    var newBids = orderBook.Bids ?? dirtyOrderBook.Bids;
                    var newAsks = orderBook.Asks ?? dirtyOrderBook.Asks;

                    var newOrderBook = new OrderBook(orderBook.Source, orderBook.AssetPair, newBids, newAsks, orderBook.Timestamp);
                    _dirtyLykkeOrderBooks[assetPairName] = newOrderBook;
                }
            }

            MoveFromDirtyToMain(orderBook.AssetPair.Name);

            return Task.CompletedTask;
        }

        private void Initialize()
        {
            InitializeAssets();
            InitializeAssetPairs();

            Task.Run(async () =>
            {
                await InitializeOrderBooks();
            })
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _log.Error(t.Exception, "Something went wrong during order books initialization from cache provider.");
            });
        }

        private void InitializeAssets()
        {
            var assets = _assetsService.AssetGetAll();
            foreach (var asset in assets)
            {
                var name = GetShortestName(asset.Id, asset.Name, asset.DisplayId);
                _lykkeAssets[name] = name;
            }

            _log.Info($"Initialized {_lykkeAssets.Count} of {assets.Count} assets.");
        }

        private void InitializeAssetPairs()
        {
            var assets = _assetsService.AssetGetAll();
            var lykkeAssetPairs = _assetsService.AssetPairGetAll();
            foreach (var lykkeAssetPair in lykkeAssetPairs)
            {
                var baseAsset = assets.SingleOrDefault(x => x.Id == lykkeAssetPair.BaseAssetId);
                var quoteAsset = assets.SingleOrDefault(x => x.Id == lykkeAssetPair.QuotingAssetId);

                if (baseAsset == null || quoteAsset == null)
                    continue;

                var baseAssetName = GetShortestName(baseAsset.Id, baseAsset.Name, baseAsset.DisplayId);
                var quoteAssetName = GetShortestName(quoteAsset.Id, quoteAsset.Name, quoteAsset.DisplayId);

                var newAssetPair = new AssetPair(baseAssetName, quoteAssetName, lykkeAssetPair.Accuracy, lykkeAssetPair.InvertedAccuracy);
                _lykkeAssetIdsAssetPairs[lykkeAssetPair.Id] = newAssetPair;
                _lykkeAssetNamesAssetPairs[newAssetPair.Name] = newAssetPair;
            }

            _log.Info($"Initialized {_lykkeAssetIdsAssetPairs.Count} of {lykkeAssetPairs.Count} asset pairs.");
        }

        private async Task InitializeOrderBooks()
        {
            foreach (var assetIdAssetPair in _lykkeAssetIdsAssetPairs)
            {
                CacheProviderOrderBook providerOrderBook = null;
                try
                {
                    providerOrderBook = await _orderBookProviderClient.GetOrderBookAsync(assetIdAssetPair.Key);
                }
                catch (Exception)
                {
                    // Some order books can't be found by asset pair id
                }

                if (providerOrderBook == null)
                    continue;

                var orderBook = Convert(providerOrderBook);
                AddOrderBookFromCacheProvider(orderBook);
            }

            var lykkeOrderBooksCount = _allOrderBooks.Values.Count(x => x.Source.Equals(LykkeExchangeName, StringComparison.InvariantCultureIgnoreCase));

            _log.Info($"Initialized {lykkeOrderBooksCount} of {_lykkeAssetIdsAssetPairs.Count} order books.");
        }

        private void AddOrderBookFromCacheProvider(OrderBook orderBook)
        {
            var assetPairName = orderBook.AssetPair.Name;

            if (!_lykkeAssetNamesAssetPairs.ContainsKey(assetPairName))
                return;

            lock (_syncLykke)
            {
                if (!_dirtyLykkeOrderBooks.ContainsKey(assetPairName))
                {
                    _dirtyLykkeOrderBooks.Add(assetPairName, orderBook);
                }
                else
                {
                    // Update half only if it doesn't exist
                    var dirtyOrderBook = _dirtyLykkeOrderBooks[assetPairName];

                    var newBids = dirtyOrderBook.Bids ?? orderBook.Bids;
                    var newAsks = dirtyOrderBook.Asks ?? orderBook.Asks;

                    var newOrderBook = new OrderBook(orderBook.Source, orderBook.AssetPair, newBids, newAsks, orderBook.Timestamp);
                    _dirtyLykkeOrderBooks[assetPairName] = newOrderBook;
                }
            }

            MoveFromDirtyToMain(assetPairName);
        }

        private void MoveFromDirtyToMain(string assetPairId)
        {
            lock (_syncLykke)
            {
                var dirtyOrderBook = _dirtyLykkeOrderBooks[assetPairId];

                if (dirtyOrderBook.Asks != null && dirtyOrderBook.Bids != null)
                {
                    var isValid = true;

                    // Only if both bids and asks not empty
                    if (dirtyOrderBook.Asks.Any() && dirtyOrderBook.Bids.Any())
                    {
                        isValid = dirtyOrderBook.Asks.Min(o => o.Price) >
                                  dirtyOrderBook.Bids.Max(o => o.Price);
                    }

                    if (isValid)
                    {
                        var key = GetKeyForOrderBooks(dirtyOrderBook);
                        _allOrderBooks[key] = new OrderBook(dirtyOrderBook.Source, dirtyOrderBook.AssetPair, dirtyOrderBook.Bids,
                                dirtyOrderBook.Asks, dirtyOrderBook.Timestamp);
                    }
                }
            }


        }

        private OrderBook Convert(CacheProviderOrderBook orderBook)
        {
            var bids = new List<VolumePrice>();
            var asks = new List<VolumePrice>();

            foreach (var limitOrder in orderBook.Prices)
            {
                // Filter out negative or zero prices and zero volumes
                if (limitOrder.Price <= 0 || (decimal)limitOrder.Volume == 0)
                    continue;

                if (limitOrder.Volume > 0)
                    bids.Add(new VolumePrice((decimal)limitOrder.Price, (decimal)limitOrder.Volume));
                else
                    asks.Add(new VolumePrice((decimal)limitOrder.Price, Math.Abs((decimal)limitOrder.Volume)));
            }

            var assetPair = _lykkeAssetIdsAssetPairs[orderBook.AssetPair];
            var result = new OrderBook(LykkeExchangeName, assetPair, bids, asks, orderBook.Timestamp);

            return result;
        }

        private static string GetShortestName(string id, string name, string displayId)
        {
            var allNames = new List<string> { id, name, displayId };
            return allNames.Where(x => x != null).MinBy(x => x.Length);
        }

        private static string GetKeyForOrderBooks(OrderBook orderBook)
        {
            return $"{orderBook.Source}-{orderBook.AssetPair.Name}";
        }
    }
}
