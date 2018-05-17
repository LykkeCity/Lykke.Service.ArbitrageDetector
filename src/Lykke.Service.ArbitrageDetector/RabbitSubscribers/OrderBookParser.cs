using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lykke.Service.ArbitrageDetector.RabbitSubscribers
{
    internal sealed class OrderBookParser
    {
        private readonly ILog _log;

        public OrderBookParser(ILog log)
        {
            _log = log;
        }

        public OrderBook Parse(byte[] data)
        {
            OrderBook result = null;

            var message = System.Text.Encoding.Default.GetString(data);
            try
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                var orderBook = JsonConvert.DeserializeObject<OrderBook>(message, serializerSettings);

                result = orderBook;
            }
            catch(JsonSerializationException ex)
            {
                _log.WriteErrorAsync(nameof(OrderBookParser), nameof(Parse), ex);
            }

            return result;
        }
    }
}
