﻿using System;

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
        public AssetPair Target { get; set; }

        /// <summary>
        /// Cross asset pair.
        /// </summary>
        public AssetPair Source { get; set; }

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
        public decimal Spread { get; set; }

        /// <summary>
        /// Base side
        /// </summary>
        public string TargetSide { get; set; }

        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; set; }

        /// <summary>
        /// Volume
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Volume in USD
        /// </summary>
        public decimal? VolumeInUsd { get; set; }

        /// <summary>
        /// PnL
        /// </summary>
        public decimal PnL { get; set; }

        /// <summary>
        /// PnL in USD
        /// </summary>
        public decimal? PnLInUsd { get; set; }

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
        public decimal? SynthAsk { get; set; }

        /// <summary>
        /// Cross bid
        /// </summary>
        public decimal? SynthBid { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
