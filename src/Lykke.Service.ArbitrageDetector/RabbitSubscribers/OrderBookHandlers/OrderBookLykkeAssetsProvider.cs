using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.Assets.Client;

namespace Lykke.Service.ArbitrageDetector.RabbitSubscribers.OrderBookHandlers
{
    internal sealed class OrderBookLykkeAssetsProvider
    {
        private readonly string _lykkeExchangeName = "lykke";

        private readonly IAssetsService _assetsService;
        private readonly ILog _log;

        public OrderBookLykkeAssetsProvider(IAssetsService assetsService, ILog log)
        {
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task ProvideAssetsIfLykke(OrderBook orderBook)
        {
            var assetPirs = await _assetsService.AssetPairGetAllWithHttpMessagesAsync();

            var good = assetPirs.Body.Where(x => x.Name.Contains("/")).ToList();

            var temp = 0;
        }
    }
}
