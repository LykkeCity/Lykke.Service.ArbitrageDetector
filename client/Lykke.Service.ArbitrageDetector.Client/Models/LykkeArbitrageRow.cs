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
        public AssetPair BaseAssetPair { get; set; }

        /// <summary>
        /// Cross asset pair.
        /// </summary>
        public AssetPair CrossAssetPair { get; set; }

        /// <summary>
        /// Count of cross pairs.
        /// </summary>
        public int CrossPairsCount { get; set; }

        /// <summary>
        /// Count of synthetic order books.
        /// </summary>
        public int SynthOrderBooksCount { get; set; }

        [Obsolete]
        public int CrossRatesCount { get; set; }

        /// <summary>
        /// Spread
        /// </summary>
        public decimal Spread { get; set; }

        /// <summary>
        /// Base side
        /// </summary>
        public string BaseSide { get; set; }

        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; set; }

        /// <summary>
        /// Volume
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Base ask
        /// </summary>
        public decimal? BaseAsk { get; set; }

        /// <summary>
        /// Base bid
        /// </summary>
        public decimal? BaseBid { get; set; }

        /// <summary>
        /// Cross ask
        /// </summary>
        public decimal? CrossAsk { get; set; }

        /// <summary>
        /// Cross bid
        /// </summary>
        public decimal? CrossBid { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
