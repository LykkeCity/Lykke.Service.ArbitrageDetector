using System;
using System.Collections.Generic;
using System.Linq;
using DomainOrderBook = Lykke.Service.ArbitrageDetector.Core.Domain.OrderBook;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents an order book or depth of market.
    /// </summary>
    public class OrderBook
    {
        /// <summary>
        /// Exchange name.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; set; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; protected set; }

        /// <summary>
        /// Asking prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Asks { get; }

        /// <summary>
        /// Bidding prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Bids { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <param name="timestamp"></param>
        /// <param name="asks"></param>
        /// <param name="bids"></param>
        public OrderBook(string source, AssetPair assetPair, DateTime timestamp, IReadOnlyCollection<VolumePrice> asks, IReadOnlyCollection<VolumePrice> bids)
        {
            Source = string.IsNullOrWhiteSpace(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = assetPair;
            Timestamp = timestamp;
            Asks = asks ?? throw new ArgumentNullException(nameof(asks));
            Bids = bids ?? throw new ArgumentNullException(nameof(bids));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain"></param>
        public OrderBook(DomainOrderBook domain)
        {
            Source = domain.Source;
            AssetPair = new AssetPair(domain.AssetPair);
            Timestamp = domain.Timestamp;
            Asks = domain.Asks.Select(x => new VolumePrice(x)).ToList();
            Bids = domain.Bids.Select(x => new VolumePrice(x)).ToList();
        }
    }
}
