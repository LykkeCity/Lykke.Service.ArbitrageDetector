using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a synthetic order book.
    /// </summary>
    [Obsolete]
    public class CrossRate : OrderBook
    {
        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; }

        /// <summary>
        /// Original order books.
        /// </summary>
        public IList<OrderBook> OriginalOrderBooks { get; }

        /// <summary>
        /// Contructor.
        /// </summary>
        public CrossRate(string source, AssetPair assetPair,
            IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks,
            string conversionPath, IList<OrderBook> originalOrderBooks, DateTime timestamp)
            : base(source, new AssetPair(assetPair.Base, assetPair.Quote), bids, asks, timestamp)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentOutOfRangeException($"{nameof(assetPair)}. Base: {assetPair.Base}, Quote: {assetPair.Quote}.");

            AssetPair = assetPair;

            ConversionPath = string.IsNullOrEmpty(conversionPath)
                ? throw new ArgumentException(nameof(conversionPath))
                : conversionPath;

            OriginalOrderBooks = originalOrderBooks ?? throw new ArgumentNullException(nameof(originalOrderBooks));
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        public static string GetConversionPath(OrderBook left, OrderBook right)
        {
            return left + " * " + right;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        public static string GetConversionPath(string leftSource, string leftAssetPair, string rightSource, string rightAssetPair)
        {
            return leftSource + "-" + leftAssetPair + " * " + rightSource + "-" + rightAssetPair;
        }

        /// <summary>
        /// Formats source - source path.
        /// </summary>
        public static string GetSourcesPath(string leftSource, string rightSource)
        {
            return leftSource + "-" + rightSource;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
