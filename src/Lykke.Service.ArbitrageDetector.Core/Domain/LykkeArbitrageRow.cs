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
        public AssetPair Target { get; }

        /// <summary>
        /// Cross asset pair.
        /// </summary>
        public AssetPair Source { get; }

        /// <summary>
        /// Count of cross pairs.
        /// </summary>
        public int SourcesCount { get; set; }

        /// <summary>
        /// Count of synthetic order books.
        /// </summary>
        public int SynthsCount { get; set; }

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
        /// PnL
        /// </summary>
        public decimal PnL { get; }

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
        public decimal? SynthAsk { get; }

        /// <summary>
        /// Cross bid
        /// </summary>
        public decimal? SynthBid { get; }

        public LykkeArbitrageRow(AssetPair baseAssetPair, AssetPair crossAssetPair, decimal spread, string baseSide,
            string conversionPath, decimal volume, decimal? baseBid, decimal? baseAsk, decimal? synthBid, decimal? synthAsk, decimal? volumeInUsd, decimal pnL)
        {
            Target = baseAssetPair.IsEmpty() ? throw new ArgumentNullException(nameof(baseAssetPair)) : baseAssetPair;
            Source = crossAssetPair.IsEmpty() ? throw new ArgumentNullException(nameof(crossAssetPair)) : crossAssetPair;
            Spread = spread;
            BaseSide = string.IsNullOrWhiteSpace(baseSide) ? throw new ArgumentNullException(nameof(baseSide)) : baseSide;
            ConversionPath = string.IsNullOrWhiteSpace(conversionPath) ? throw new ArgumentNullException(nameof(conversionPath)) : conversionPath;
            Volume = volume;
            BaseAsk = baseAsk;
            BaseBid = baseBid;
            SynthAsk = synthAsk;
            SynthBid = synthBid;
            VolumeInUsd = volumeInUsd;
            PnL = pnL;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Target + "-" + Source + " : " + ConversionPath;
        }
    }
}
