using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an order book.
    /// </summary>
    public sealed class OrderBook
    {
        /// <summary>
        /// Name of an exchange.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Name of an asset pair.
        /// </summary>
        [JsonProperty("asset")]
        public string AssetPair { get; }

        /// <summary>
        /// The time when the current order book was actual.
        /// </summary>
        public DateTime Timestamp { get; }
        
        /// <summary>
        /// Asks of the current order book.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Asks { get; }

        /// <summary>
        /// Bids of the current order book.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Bids { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source">Exchange name.</param>
        /// <param name="assetPair">Asset pair.</param>
        /// <param name="bids">Bids.</param>
        /// <param name="asks">Asks.</param>
        /// <param name="timestamp">Timestamp.</param>
        public OrderBook(string source, string assetPair, IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks, DateTime timestamp)
        {
            Source = string.IsNullOrEmpty(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = string.IsNullOrEmpty(assetPair) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
            Asks = asks.OrderBy(x => x.Price).ToList();
            Bids = bids.OrderByDescending(x => x.Price).ToList();
            Timestamp = timestamp;
        }

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
