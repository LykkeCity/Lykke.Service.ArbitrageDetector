using System;
using System.Collections.Generic;
using System.Linq;
using DomainSynthOrderBook = Lykke.Service.ArbitrageDetector.Core.Domain.SynthOrderBook;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents a synthetic order book.
    /// </summary>
    public class SynthOrderBook : OrderBook
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
        public SynthOrderBook(string source, AssetPair assetPair,
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
        public SynthOrderBook(DomainSynthOrderBook domain)
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
