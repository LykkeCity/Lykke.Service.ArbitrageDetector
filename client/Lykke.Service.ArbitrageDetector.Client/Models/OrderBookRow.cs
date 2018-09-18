using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a synthetic order book.
    /// </summary>
    public class OrderBookRow
    {
        /// <summary>
        /// Conversion path.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; set; }

        /// <summary>
        /// Best bid.
        /// </summary>
        public VolumePrice? BestBid { get; set; }

        /// <summary>
        /// Best ask.
        /// </summary>
        public VolumePrice? BestAsk { get; set; }

        /// <summary>
        /// Comulative volume of all bids.
        /// </summary>
        public decimal BidsVolume { get; set; }

        /// <summary>
        /// Comulative volume of all asks.
        /// </summary>
        public decimal AsksVolume { get; set; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
