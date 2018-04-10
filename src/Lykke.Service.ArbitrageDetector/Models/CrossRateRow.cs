using System;
using System.Linq;
using MoreLinq;
using DomainCrossRate = Lykke.Service.ArbitrageDetector.Core.Domain.CrossRate;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents a cross rate.
    /// </summary>
    public class CrossRateRow
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
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <param name="bestBid"></param>
        /// <param name="bestAsk"></param>
        /// <param name="conversionPath"></param>
        /// <param name="timestamp"></param>
        public CrossRateRow(string source, AssetPair assetPair, VolumePrice? bestBid, VolumePrice? bestAsk, string conversionPath, DateTime timestamp)
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
        /// <param name="domain"></param>
        public CrossRateRow(DomainCrossRate domain)
        {
            Source = domain.Source;
            AssetPair = new AssetPair(domain.AssetPair);
            BestBid = domain.Bids.Any() ? new VolumePrice(domain.Bids.MinBy(x => x.Price)) : (VolumePrice?)null;
            BestAsk = domain.Asks.Any() ? new VolumePrice(domain.Asks.MinBy(x => x.Price)) : (VolumePrice?)null;
            ConversionPath = domain.ConversionPath;
            Timestamp = domain.Timestamp;
        }
    }
}
