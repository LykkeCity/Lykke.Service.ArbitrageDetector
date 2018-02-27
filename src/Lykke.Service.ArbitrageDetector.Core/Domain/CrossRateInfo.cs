using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct CrossRateInfo
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
        public OrderBook OriginalOrderBook { get; }

        public CrossRateInfo(string source, string assetPair, decimal bestBid, decimal bestAsk, string conversionPath,
            OrderBook originalOrderBook) : this()
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            AssetPair = assetPair ?? throw new ArgumentNullException(nameof(assetPair));
            BestBid = bestBid;
            BestAsk = bestAsk;
            ConversionPath = conversionPath ?? throw new ArgumentNullException(nameof(conversionPath));
            OriginalOrderBook = originalOrderBook ?? throw new ArgumentNullException(nameof(originalOrderBook));
        }
    }
}
