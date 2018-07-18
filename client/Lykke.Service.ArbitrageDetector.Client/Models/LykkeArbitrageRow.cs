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
        /// Count of cross pairs.
        /// </summary>
        public int CrossPairsCount { get; }

        /// <summary>
        /// Count of synthetic order books.
        /// </summary>
        public int SynthOrderBooksCount { get; }

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

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
