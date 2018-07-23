using System;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class LykkeArbitrageRow
    {
        /// <summary>
        /// Base asset pair.
        /// </summary>
        [Obsolete]
        public AssetPair BaseAssetPair { get; }

        /// <summary>
        /// Target asset pair.
        /// </summary>
        public AssetPair Target { get; }

        /// <summary>
        /// Cross asset pair.
        /// </summary>
        [Obsolete]
        public AssetPair CrossAssetPair { get; }

        /// <summary>
        /// Source asset pair.
        /// </summary>
        public AssetPair Source { get; }

        /// <summary>
        /// Count of cross pairs.
        /// </summary>
        [Obsolete]
        public int CrossPairsCount { get; }

        /// <summary>
        /// Count of source pairs.
        /// </summary>
        public int SourcesCount { get; }

        /// <summary>
        /// Count of synthetic order books.
        /// </summary>
        public int SynthsCount { get; }

        /// <summary>
        /// Count of synthetic order books.
        /// </summary>
        [Obsolete]
        public int CrossRatesCount { get; }

        /// <summary>
        /// Spread
        /// </summary>
        public decimal Spread { get; }

        /// <summary>
        /// Base side
        /// </summary>
        [Obsolete]
        public string BaseSide { get; }

        /// <summary>
        /// Target side
        /// </summary>
        public string TargetSide { get; }

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
        /// PnL in USD
        /// </summary>
        public decimal? PnLInUsd { get; }

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
        [Obsolete]
        public decimal? CrossAsk { get; }

        /// <summary>
        /// Cross bid
        /// </summary>
        [Obsolete]
        public decimal? CrossBid { get; }

        /// <summary>
        /// Synth ask
        /// </summary>
        public decimal? SynthAsk { get; }

        /// <summary>
        /// Synth bid
        /// </summary>
        public decimal? SynthBid { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain"></param>
        public LykkeArbitrageRow(Core.Domain.LykkeArbitrageRow domain)
        {
            BaseAssetPair = new AssetPair(domain.Target);
            CrossAssetPair = new AssetPair(domain.Source);
            Target = new AssetPair(domain.Target);
            Source = new AssetPair(domain.Source);
            CrossPairsCount = domain.SourcesCount;
            SourcesCount = domain.SourcesCount;
            CrossRatesCount = domain.SynthsCount;
            SynthsCount = domain.SynthsCount;
            Spread = Math.Round(domain.Spread, 8);
            BaseSide = domain.TargetSide;
            TargetSide = domain.TargetSide;
            ConversionPath = domain.ConversionPath.Replace("lykke-", "");
            Volume = Math.Round(domain.Volume, 8);
            VolumeInUsd = domain.VolumeInUsd.HasValue ? Math.Round(domain.VolumeInUsd.Value, 8) : (decimal?)null;
            BaseAsk = domain.BaseAsk.HasValue ? Math.Round(domain.BaseAsk.Value, 8) : (decimal?)null;
            BaseBid = domain.BaseBid.HasValue ? Math.Round(domain.BaseBid.Value, 8) : (decimal?)null;
            CrossAsk = domain.SynthAsk.HasValue ? Math.Round(domain.SynthAsk.Value, 8) : (decimal?)null;
            CrossBid = domain.SynthBid.HasValue ? Math.Round(domain.SynthBid.Value, 8) : (decimal?)null;
            SynthAsk = domain.SynthAsk.HasValue ? Math.Round(domain.SynthAsk.Value, 8) : (decimal?)null;
            SynthBid = domain.SynthBid.HasValue ? Math.Round(domain.SynthBid.Value, 8) : (decimal?)null;
            PnL = domain.PnL;
            PnLInUsd = domain.PnLInUsd;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
