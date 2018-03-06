using System;
using System.Collections.Generic;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an order book.
    /// </summary>
    public sealed class OrderBook
    {
        /// <summary>
        /// A name of an exchange.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// A name of an asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The time when the current order book was actual.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Asks of the current order book.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Asks { get; set; }

        /// <summary>
        /// Bids of the current order book.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Bids { get; set; }

        /// <summary>
        /// Returns the maximum bid from this order book.
        /// </summary>
        /// <returns>The highest bid price.</returns>
        public decimal GetBestBid()
        {
            return Bids.MaxBy(x => x.Price).Price;
        }

        /// <summary>
        /// Returns the minimum ask from this order book.
        /// </summary>
        /// <returns>The lowest asking price.</returns>
        public decimal GetBestAsk()
        {
            return Asks.MinBy(x => x.Price).Price;
        }
    }
}
