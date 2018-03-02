using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class OrderBookProcessor : TimerPeriod, IOrderBookProcessor
    {
        private readonly ILog _log;
        private readonly IArbitrageCalculator _arbitrageCalculator;

        public OrderBookProcessor(
            ILog log,
            IArbitrageCalculator arbitrageCalculator,
            IShutdownManager shutdownManager)
            : base((int)TimeSpan.FromMinutes(90).TotalMilliseconds, log)
        {
            _log = log;
            _arbitrageCalculator = arbitrageCalculator;
            shutdownManager.Register(this);
        }

        public void Process(byte[] data)
        {
            var dataStr = System.Text.Encoding.Default.GetString(data);
            OrderBook orderBook = null;
            try
            {
                orderBook = JsonConvert.DeserializeObject<OrderBook>(dataStr);
            }
            catch(JsonSerializationException ex)
            {
                _log.WriteErrorAsync(nameof(OrderBookProcessor), nameof(Execute), ex);
            }

            if (orderBook != null)
            {
                _arbitrageCalculator.Process(orderBook);

                _log.WriteInfo(nameof(OrderBookProcessor), nameof(Process), $"Exchange: {orderBook.Source}, assetPair: {orderBook.AssetPairId}");
            }
        }

        public override async Task Execute()
        {
            await _log.WriteInfoAsync(nameof(OrderBookProcessor), nameof(Execute), $"OrderBookProcessor.Execute()");
        }
    }
}
