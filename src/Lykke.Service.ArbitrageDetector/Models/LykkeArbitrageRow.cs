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
        public AssetPair BaseAssetPair { get; }

        /// <summary>
        /// Cross asset pair.
        /// </summary>
        public AssetPair CrossAssetPair { get; }

        /// <summary>
        /// Count of cross pairs.
        /// </summary>
        public int CrossPairsCount { get; }

        /// <summary>
        /// Count of synthetic order books.
        /// </summary>
        public int SynthOrderBooksCount { get; }

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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain"></param>
        public LykkeArbitrageRow(Core.Domain.LykkeArbitrageRow domain)
        {
            BaseAssetPair = new AssetPair(domain.BaseAssetPair);
            CrossAssetPair = new AssetPair(domain.CrossAssetPair);
            CrossPairsCount = domain.CrossPairsCount;
            SynthOrderBooksCount = domain.SynthOrderBooksCount;
            Spread = Math.Round(domain.Spread, 8);
            BaseSide = domain.BaseSide;
            ConversionPath = domain.ConversionPath.Replace("lykke-", "");
            Volume = Math.Round(domain.Volume, 8);
            VolumeInUsd = domain.VolumeInUsd.HasValue ? Math.Round(domain.VolumeInUsd.Value, 8) : (decimal?)null;
            BaseAsk = domain.BaseAsk.HasValue ? Math.Round(domain.BaseAsk.Value, 8) : (decimal?)null;
            BaseBid = domain.BaseBid.HasValue ? Math.Round(domain.BaseBid.Value, 8) : (decimal?)null;
            CrossAsk = domain.CrossAsk.HasValue ? Math.Round(domain.CrossAsk.Value, 8) : (decimal?)null;
            CrossBid = domain.CrossBid.HasValue ? Math.Round(domain.CrossBid.Value, 8) : (decimal?)null;

            CrossRatesCount = domain.SynthOrderBooksCount;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
