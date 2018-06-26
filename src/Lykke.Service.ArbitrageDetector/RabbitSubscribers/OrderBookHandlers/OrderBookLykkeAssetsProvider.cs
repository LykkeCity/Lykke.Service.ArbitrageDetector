using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.Assets.Client;

namespace Lykke.Service.ArbitrageDetector.RabbitSubscribers.OrderBookHandlers
{
    internal sealed class OrderBookLykkeAssetsProvider
    {
        private const string LykkeExchangeName = "lykke";
        
        private readonly Dictionary<string, AssetPair> _assetPairs = new Dictionary<string, AssetPair>();

        private readonly IAssetsService _assetsService;
        private readonly ILog _log;

        public OrderBookLykkeAssetsProvider(IAssetsService assetsService, ILog log)
        {
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            Task.Run(() => InitAssetsPairs()).Wait();
        }

        public async Task ProvideAssetsIfLykke(OrderBook orderBook)
        {
            if (orderBook.Source == LykkeExchangeName && _assetPairs.Count > 0)
            {
                var key = _assetPairs.Keys.SingleOrDefault(x => x == orderBook.AssetPairStr);
                if (key != null)
                {
                    orderBook.AssetPair = _assetPairs[key];
                }
            }
        }

        private async Task InitAssetsPairs()
        {
            var allAssetPirs = await _assetsService.AssetPairGetAllWithHttpMessagesAsync();

            var goodAssetPairs = allAssetPirs.Body
                .Where(x => x.Name != null && x.Name.Contains("/")).ToList();

            foreach (var assetPair in goodAssetPairs)
            {
                var key = assetPair.Name.Replace("/", "");
                var baseQuote = assetPair.Name.Split("/");
                var @base = baseQuote[0];
                var quote = baseQuote[1];
                if (!_assetPairs.ContainsKey(key))
                    _assetPairs.Add(key, new AssetPair(@base, quote));
            }
        }
 
    }
}
