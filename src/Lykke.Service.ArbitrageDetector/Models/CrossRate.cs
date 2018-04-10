using System;
using System.Collections.Generic;
using System.Linq;
using DomainCrossRate = Lykke.Service.ArbitrageDetector.Core.Domain.CrossRate;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents a cross rate.
    /// </summary>
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
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <param name="bids"></param>
        /// <param name="asks"></param>
        /// <param name="conversionPath"></param>
        /// <param name="originalOrderBooks"></param>
        /// <param name="timestamp"></param>
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
        /// Constructor from domain object.
        /// </summary>
        /// <param name="domain"></param>
        public CrossRate(DomainCrossRate domain)
            : this(domain.Source, new AssetPair(domain.AssetPair),
                domain.Bids.Select(x => new VolumePrice(x)).ToList(), domain.Asks.Select(x => new VolumePrice(x)).ToList(),
                domain.ConversionPath, domain.OriginalOrderBooks.Select(x => new OrderBook(x)).ToList(), domain.Timestamp)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
