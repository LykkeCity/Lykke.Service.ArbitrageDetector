using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an order book or depth of market.
    /// </summary>
    public class OrderBook
    {
        /// <summary>
        /// Exchange name.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; set; }

        /// <summary>
        /// Bidding prices and volumes.
        /// </summary>
        public IReadOnlyList<VolumePrice> Bids { get; set; }

        /// <summary>
        /// Asking prices and volumes.
        /// </summary>
        public IReadOnlyList<VolumePrice> Asks { get; set; }

        /// <summary>
        /// Best bid.
        /// </summary>
        [JsonIgnore]
        public VolumePrice? BestBid => Bids.Any() ? Bids.MaxBy(x => x.Price) : (VolumePrice?)null;

        /// <summary>
        /// Best ask.
        /// </summary>
        [JsonIgnore]
        public VolumePrice? BestAsk => Asks.Any() ? Asks.MinBy(x => x.Price) : (VolumePrice?)null;

        /// <summary>
        /// All bids volume.
        /// </summary>
        [JsonIgnore]
        public decimal BidsVolume => Bids.Sum(x => x.Volume);

        /// <summary>
        /// All asks volume.
        /// </summary>
        [JsonIgnore]
        public decimal AsksVolume => Asks.Sum(x => x.Volume);

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source}-{AssetPair.Name}";
        }
    }
}
