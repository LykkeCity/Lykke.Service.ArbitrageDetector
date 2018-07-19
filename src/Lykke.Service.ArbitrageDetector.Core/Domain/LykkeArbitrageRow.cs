using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
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
        /// Count of cross pairs.
        /// </summary>
        public int CrossPairsCount { get; set; }

        /// <summary>
        /// Count of synthetic order books.
        /// </summary>
        public int SynthOrderBooksCount { get; set; }

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
        /// Volume in USD
        /// </summary>
        public decimal? VolumeInUsd { get; }

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
            string conversionPath, decimal volume, decimal? baseBid, decimal? baseAsk, decimal? crossBid, decimal? crossAsk, decimal? volumeInUsd)
        {
            BaseAssetPair = baseAssetPair.IsEmpty() ? throw new ArgumentNullException(nameof(baseAssetPair)) : baseAssetPair;
            CrossAssetPair = crossAssetPair.IsEmpty() ? throw new ArgumentNullException(nameof(crossAssetPair)) : crossAssetPair;
            Spread = spread;
            BaseSide = string.IsNullOrWhiteSpace(baseSide) ? throw new ArgumentNullException(nameof(baseSide)) : baseSide;
            ConversionPath = string.IsNullOrWhiteSpace(conversionPath) ? throw new ArgumentNullException(nameof(conversionPath)) : conversionPath;
            Volume = volume;
            BaseAsk = baseAsk;
            BaseBid = baseBid;
            CrossAsk = crossAsk;
            CrossBid = crossBid;
            VolumeInUsd = volumeInUsd;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return BaseAssetPair + "-" + CrossAssetPair + " : " + ConversionPath;
        }
    }
}
