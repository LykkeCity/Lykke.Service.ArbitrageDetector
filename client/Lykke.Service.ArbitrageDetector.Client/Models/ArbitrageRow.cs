using System;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class ArbitrageRow
    {
        /// <summary>
        /// AssetPair.
        /// </summary>
        public AssetPair AssetPair { get; set; }

        /// <summary>
        /// Bid exchange name.
        /// </summary>
        public string BidSource { get; set; }

        /// <summary>
        /// Ask exchange name.
        /// </summary>
        public string AskSource { get; set; }

        /// <summary>
        /// Conversion path from bid.
        /// </summary>
        public string BidConversionPath { get; set; }

        /// <summary>
        /// Conversion path from ask.
        /// </summary>
        public string AskConversionPath { get; set; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; set; }

        /// <summary>
        /// Price and volume of low ask.
        /// </summary>
        public VolumePrice Ask { get; set; }

        /// <summary>
        /// Spread between ask and bid.
        /// </summary>
        public decimal Spread { get; set; }

        /// <summary>
        /// The smallest volume of ask or bid.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Potential profit or loss.
        /// </summary>
        public decimal PnL { get; set; }

        /// <summary>
        /// The time when it first appeared.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// The time when it disappeared.
        /// </summary>
        public DateTime EndedAt { get; set; }

        /// <summary>
        /// How log the arbitrage lasted.
        /// </summary>
        [JsonIgnore]
        public TimeSpan Lasted => EndedAt == default ? DateTime.UtcNow - StartedAt : EndedAt - StartedAt;

        /// <summary>
        /// Conversion path.
        /// </summary>
        [JsonIgnore]
        public string ConversionPath => Arbitrage.FormatConversionPath(BidConversionPath, AskConversionPath);

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
