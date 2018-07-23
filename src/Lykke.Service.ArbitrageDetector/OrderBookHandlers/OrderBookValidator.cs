using System;
using System.Linq;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.OrderBookHandlers
{
    internal sealed class OrderBookValidator
    {
        private readonly ILog _log;

        public OrderBookValidator(ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public bool IsValid(OrderBook orderBook)
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
                WriteToLogThrottled(orderBook);

                return false;
            }

            return true;
        }

        // TODO: Must be removed or changed to silent ignore w/o lock
        private readonly object _thisLock = new object();
        private DateTime _lastWriteToLog = DateTime.MinValue;
        public int WriteToLogDelayInMilliseconds { get; set; } = 5 * 60 * 1000;
        public void WriteToLogThrottled(OrderBook orderBook)
        {
            lock (_thisLock)
            {
                var canWriteAgain = (DateTime.Now - _lastWriteToLog).TotalMilliseconds > WriteToLogDelayInMilliseconds;
                if (canWriteAgain)
                {
                    _lastWriteToLog = DateTime.Now;
                    _log.WriteInfoAsync(nameof(OrderBookParser), nameof(IsValid), $"Invalid order book: {orderBook.Source}-{orderBook.AssetPairStr}");
                }
            }
        }
    }
}
