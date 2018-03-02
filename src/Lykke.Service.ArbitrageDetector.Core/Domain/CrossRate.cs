using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class CrossRate
    {
        /// Exchange name
        public string Source { get; }

        /// Wanted asset pair name
        public string AssetPair { get; }

        /// Best conversion bid
        public decimal BestBid { get; }

        /// Best conversion ask
        public decimal BestAsk { get; }

        /// Conversion path of cross rate
        public string ConversionPath { get; }

        /// Creation time stamp
        public DateTime Timestamp { get; }

        /// Original order book for cross rate calculation
        public IList<OrderBook> OriginalOrderBooks { get; }

        public CrossRate(string source, string assetPair, decimal bestBid, decimal bestAsk, string conversionPath,
            IList<OrderBook> originalOrderBooks)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            AssetPair = assetPair ?? throw new ArgumentNullException(nameof(assetPair));
            BestBid = bestBid;
            BestAsk = bestAsk;
            ConversionPath = conversionPath ?? throw new ArgumentNullException(nameof(conversionPath));
            Timestamp = DateTime.UtcNow;
            OriginalOrderBooks = originalOrderBooks ?? throw new ArgumentNullException(nameof(originalOrderBooks));
        }
    }
}
