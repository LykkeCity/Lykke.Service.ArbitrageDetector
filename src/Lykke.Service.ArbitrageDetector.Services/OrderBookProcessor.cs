using System.Linq;
using Common;
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
            var message = System.Text.Encoding.Default.GetString(data);
            try
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                var orderBook = JsonConvert.DeserializeObject<OrderBook>(message, serializerSettings);
                if (orderBook != null)
                {
                    // Check if price or volume is 0
                    if ((orderBook.Bids != null && (orderBook.Bids.Any(x => x.Price == 0) || orderBook.Bids.Any(x => x.Volume == 0)))
                     || (orderBook.Asks != null && (orderBook.Asks.Any(x => x.Price == 0) || orderBook.Asks.Any(x => x.Volume == 0))))
                    {
                        _log.WriteInfoAsync(nameof(OrderBookProcessor), nameof(Process),
                            $"original message: {message}, parsed orderBook: {orderBook.ToJson()}");
                    }
                    else
                    {
                        _arbitrageDetectorService.Process(orderBook);
                    }
                }
            }
            catch(JsonSerializationException ex)
            {
                _log.WriteErrorAsync(nameof(OrderBookProcessor), nameof(Process), ex);
            }
        }
    }
}
