using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class OrderBookProcessor : IOrderBookProcessor
    {
        private readonly ILog _log;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;

        public OrderBookProcessor(
            ILog log,
            IArbitrageDetectorService arbitrageDetectorService)
        {
            _log = log;
            _arbitrageDetectorService = arbitrageDetectorService;
        }

        public void Process(byte[] data)
        {
            var dataStr = System.Text.Encoding.Default.GetString(data);
            OrderBook orderBook = null;
            try
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                orderBook = JsonConvert.DeserializeObject<OrderBook>(dataStr, serializerSettings);
            }
            catch(JsonSerializationException ex)
            {
                _log.WriteErrorAsync(nameof(OrderBookProcessor), nameof(Process), ex);
            }

            if (orderBook != null)
            {
                _arbitrageDetectorService.Process(orderBook);
            }
        }
    }
}
