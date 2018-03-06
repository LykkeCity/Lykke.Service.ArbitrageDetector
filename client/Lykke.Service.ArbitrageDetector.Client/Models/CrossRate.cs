using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a calculated cross rate.
    /// </summary>
    public class CrossRate
    {
        /// <summary>
        /// Exchanges of the current cross rate.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Calculated asset pair.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Bid of the current cross rate.
        /// </summary>
        public decimal Bid { get; set; }

        /// <summary>
        /// Ask of the current cross rate.
        /// </summary>
        public decimal Ask { get; set; }

        /// <summary>
        /// The path from which current cross rate was calculated.
        /// </summary>
        public string ConversionPath { get; set; }

        /// <summary>
        /// The time when the current cross rate was calculated.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// The original order books from which the current cross rate was calculated.
        /// </summary>
        public IList<OrderBook> OriginalOrderBooks { get; set; }
    }
}
