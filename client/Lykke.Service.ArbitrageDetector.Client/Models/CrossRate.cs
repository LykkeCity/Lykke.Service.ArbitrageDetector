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
        public string Source { get; }

        /// <summary>
        /// Calculated asset pair.
        /// </summary>
        public string AssetPair { get; }

        /// <summary>
        /// Bid of the current cross rate.
        /// </summary>
        public decimal Bid { get; }

        /// <summary>
        /// Ask of the current cross rate.
        /// </summary>
        public decimal Ask { get; }

        /// <summary>
        /// Path from which current cross rate was calculated.
        /// </summary>
        public string ConversionPath { get; }

        /// <summary>
        /// Time when the current cross rate was calculated.
        /// </summary>
        public DateTime Timestamp { get; }
        
        /// <summary>
        /// Original order books from which the current cross rate was calculated.
        /// </summary>
        public IList<OrderBook> OriginalOrderBooks { get; }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="source">Exchange name.</param>
        /// <param name="assetPair">Asset pair.</param>
        /// <param name="bid">Bid.</param>
        /// <param name="ask">Ask.</param>
        /// <param name="conversionPath">Conversion path.</param>
        /// <param name="originalOrderBooks">Original order books.</param>
        public CrossRate(string source, string assetPair, decimal bid, decimal ask, string conversionPath, IList<OrderBook> originalOrderBooks)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            AssetPair = assetPair ?? throw new ArgumentNullException(nameof(assetPair));
            Bid = bid;
            Ask = ask;
            ConversionPath = conversionPath ?? throw new ArgumentNullException(nameof(conversionPath));
            Timestamp = DateTime.UtcNow;
            OriginalOrderBooks = originalOrderBooks ?? throw new ArgumentNullException(nameof(originalOrderBooks));
        }
    }
}
