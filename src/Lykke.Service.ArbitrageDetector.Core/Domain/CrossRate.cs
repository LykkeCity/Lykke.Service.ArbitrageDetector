using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class CrossRate
    {
        public string Source { get; }

        public string AssetPair { get; }

        public decimal Bid { get; }

        public decimal Ask { get; }

        public string ConversionPath { get; }

        public DateTime Timestamp { get; }

        public IList<OrderBook> OriginalOrderBooks { get; }

        public CrossRate(string source, string assetPair, decimal bid, decimal ask, string conversionPath, IList<OrderBook> originalOrderBooks)
        {
            Source = string.IsNullOrEmpty(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = string.IsNullOrEmpty(assetPair) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
            Bid = bid;
            Ask = ask;
            ConversionPath = string.IsNullOrEmpty(conversionPath) ? throw new ArgumentNullException(nameof(conversionPath)) : conversionPath;
            Timestamp = DateTime.UtcNow;
            OriginalOrderBooks = originalOrderBooks ?? throw new ArgumentNullException(nameof(originalOrderBooks));
        }
    }
}
