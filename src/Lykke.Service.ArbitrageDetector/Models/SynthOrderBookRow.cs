using System;
using DomainSynthOrderBook = Lykke.Service.ArbitrageDetector.Core.Domain.SynthOrderBook;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents a synthetic order book.
    /// </summary>
    public class SynthOrderBookRow
    {
        /// <summary>
        /// Conversion path.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; }

        /// <summary>
        /// Best bid.
        /// </summary>
        public VolumePrice? BestBid { get; }

        /// <summary>
        /// Best ask.
        /// </summary>
        public VolumePrice? BestAsk { get; }

        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp{ get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SynthOrderBookRow(string source, AssetPair assetPair, VolumePrice? bestBid, VolumePrice? bestAsk, string conversionPath, DateTime timestamp)
        {
            Source = string.IsNullOrWhiteSpace(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = assetPair;
            BestBid = bestBid;
            BestAsk = bestAsk;
            ConversionPath = string.IsNullOrWhiteSpace(conversionPath) ? throw new ArgumentNullException(nameof(conversionPath)) : conversionPath;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SynthOrderBookRow(DomainSynthOrderBook domain)
        {
            Source = domain.Source;
            AssetPair = new AssetPair(domain.AssetPair);
            BestBid = VolumePrice.FromDomain(domain.BestBid);
            BestAsk = VolumePrice.FromDomain(domain.BestAsk);
            ConversionPath = domain.ConversionPath;
            Timestamp = domain.Timestamp;
        }
    }
}
