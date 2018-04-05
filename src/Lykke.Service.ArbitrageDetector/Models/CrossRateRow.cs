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
        /// Best ask.
        /// </summary>
        public VolumePrice? BestAsk { get; }

        /// <summary>
        /// Best bid.
        /// </summary>
        public VolumePrice? BestBid { get; }

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
        /// <param name="bestAsk"></param>
        /// <param name="bestBid"></param>
        /// <param name="conversionPath"></param>
        /// <param name="timestamp"></param>
        public CrossRateRow(string source, AssetPair assetPair, VolumePrice? bestAsk, VolumePrice? bestBid, string conversionPath, DateTime timestamp)
        {
            Source = string.IsNullOrWhiteSpace(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = assetPair;
            BestAsk = bestAsk;
            BestBid = bestBid;
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

            var bestAsk = domain.Asks.Any() ? domain.Asks.MinBy(x => x.Price) : (Core.Domain.VolumePrice?)null;
            var bestBid = domain.Bids.Any() ? domain.Bids.MaxBy(x => x.Price) : (Core.Domain.VolumePrice?)null;
            BestAsk = bestAsk == null ? (VolumePrice?)null : new VolumePrice(bestAsk.Value.Price, bestAsk.Value.Volume);
            BestBid = bestBid == null ? (VolumePrice?)null : new VolumePrice(bestBid.Value.Price, bestBid.Value.Volume);

            ConversionPath = domain.ConversionPath;
            Timestamp = domain.Timestamp;
        }
    }
}
