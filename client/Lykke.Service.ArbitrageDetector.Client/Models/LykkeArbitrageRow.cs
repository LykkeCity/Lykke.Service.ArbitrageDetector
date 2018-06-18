using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class LykkeArbitrageRow
    {
        /// <summary>
        /// Base asset pair.
        /// </summary>
        public AssetPair BaseAssetPair { get; }

        /// <summary>
        /// Cross asset pair.
        /// </summary>
        public AssetPair CrossAssetPair { get; }

        /// <summary>
        /// Spread
        /// </summary>
        public decimal Spread { get; }

        /// <summary>
        /// Base side
        /// </summary>
        public string BaseSide { get; }

        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; }

        /// <summary>
        /// Volume
        /// </summary>
        public decimal Volume { get; }

        /// <summary>
        /// Base ask
        /// </summary>
        public decimal? BaseAsk { get; }

        /// <summary>
        /// Base bid
        /// </summary>
        public decimal? BaseBid { get; }

        /// <summary>
        /// Cross ask
        /// </summary>
        public decimal? CrossAsk { get; }

        /// <summary>
        /// Cross bid
        /// </summary>
        public decimal? CrossBid { get; }

        public LykkeArbitrageRow(AssetPair baseAssetPair, AssetPair crossAssetPair, decimal spread, string baseSide,
            string conversionPath, decimal volume, decimal? baseBid, decimal? baseAsk, decimal? crossBid, decimal? crossAsk)
        {
            BaseAssetPair = baseAssetPair.IsEmpty() ? throw new ArgumentNullException(nameof(baseAssetPair)) : baseAssetPair;
            CrossAssetPair = crossAssetPair.IsEmpty() ? throw new ArgumentNullException(nameof(crossAssetPair)) : crossAssetPair;
            Spread = Math.Round(spread, 8);
            BaseSide = string.IsNullOrWhiteSpace(baseSide) ? throw new ArgumentNullException(nameof(baseSide)) : baseSide;
            ConversionPath = string.IsNullOrWhiteSpace(conversionPath) ? throw new ArgumentNullException(nameof(conversionPath)) : conversionPath;
            Volume = Math.Round(volume, 8);
            BaseAsk = baseAsk.HasValue ? Math.Round(baseAsk.Value, 8) : (decimal?)null;
            BaseBid = baseBid.HasValue ? Math.Round(baseBid.Value, 8) : (decimal?)null;
            CrossAsk = crossAsk.HasValue ? Math.Round(crossAsk.Value, 8) : (decimal?)null;
            CrossBid = crossBid.HasValue ? Math.Round(crossBid.Value, 8) : (decimal?)null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
