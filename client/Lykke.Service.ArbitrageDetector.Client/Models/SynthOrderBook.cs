using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a synthetic order book.
    /// </summary>
    public class SynthOrderBook : OrderBook
    {
        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; set; }

        /// <summary>
        /// Original order books.
        /// </summary>
        public IList<OrderBook> OriginalOrderBooks { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
