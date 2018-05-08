using System;
using System.Linq;
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

        public OrderBookProcessor(ILog log, IArbitrageDetectorService arbitrageDetectorService)
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
                if (CheckIfOrderBookIsValid(orderBook, message))
                {
                    _arbitrageDetectorService.Process(orderBook);
                }
            }
            catch(JsonSerializationException ex)
            {
                _log.WriteErrorAsync(nameof(OrderBookProcessor), nameof(Process), ex);
            }
        }

        // TODO: Must be removed or changed to silent ignore w/o lock
        private readonly object _thisLock = new object();
        private DateTime _lastWriteToLog = DateTime.MinValue;
        public int WriteToLogDelayInMilliseconds { get; set; } = 5 * 60 * 1000;
        private bool CheckIfOrderBookIsValid(OrderBook orderBook, string originalMessage)
        {
            if (orderBook == null)
                return false;

            // Check if price or volume is 0 or spread is negative
            var bidPriceOrVolumeEquals0 = orderBook.Bids != null && (orderBook.Bids.Any(x => x.Price == 0) || orderBook.Bids.Any(x => x.Volume == 0));
            var askPriceOrVolumeEquals0 = orderBook.Asks != null && (orderBook.Asks.Any(x => x.Price == 0) || orderBook.Asks.Any(x => x.Volume == 0));
            var spreadIsNegative = orderBook.Bids != null && orderBook.Asks != null
                                                          && orderBook.BestBid.HasValue && orderBook.BestAsk.HasValue
                                                          && (orderBook.BestBid.Value.Price > orderBook.BestAsk.Value.Price);

            if (bidPriceOrVolumeEquals0 || askPriceOrVolumeEquals0 || spreadIsNegative)
            {
                lock (_thisLock)
                {
                    var canWriteAgain = (DateTime.Now - _lastWriteToLog).TotalMilliseconds > WriteToLogDelayInMilliseconds;
                    if (canWriteAgain)
                    {
                        _lastWriteToLog = DateTime.Now;
                        _log.WriteInfoAsync(nameof(OrderBookProcessor), nameof(Process), $"original message from RabbitMq converted from byte[] to string: {originalMessage}");
                    }
                }

                return false;
            }

            return true;
        }
    }
}
