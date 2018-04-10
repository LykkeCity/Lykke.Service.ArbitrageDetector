using System;

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
        public AssetPair AssetPair { get; }

        /// <summary>
        /// Ask exchange name.
        /// </summary>
        public string AskSource { get; }

        /// <summary>
        /// Bid exchange name.
        /// </summary>
        public string BidSource { get; }

        /// <summary>
        /// Conversion path from ask.
        /// </summary>
        public string AskConversionPath { get; }

        /// <summary>
        /// Conversion path from bid.
        /// </summary>
        public string BidConversionPath { get; }

        /// <summary>
        /// Price and volume of low ask.
        /// </summary>
        public VolumePrice Ask { get; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; }

        /// <summary>
        /// Spread between ask and bid.
        /// </summary>
        public decimal Spread { get; }

        /// <summary>
        /// The smallest volume of ask or bid.
        /// </summary>
        public decimal Volume { get; }

        /// <summary>
        /// Potential profit or loss.
        /// </summary>
        public decimal PnL { get; }

        /// <summary>
        /// The time when it first appeared.
        /// </summary>
        public DateTime StartedAt { get; }

        /// <summary>
        /// The time when it disappeared.
        /// </summary>
        public DateTime EndedAt { get; set; }

        /// <summary>
        /// How log the arbitrage lasted.
        /// </summary>
        public TimeSpan Lasted => EndedAt == default ? DateTime.UtcNow - StartedAt : EndedAt - StartedAt;

        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath => Arbitrage.FormatConversionPath(AskConversionPath, BidConversionPath);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="askSource"></param>
        /// <param name="bidSource"></param>
        /// <param name="askConversionPath"></param>
        /// <param name="bidConversionPath"></param>
        /// <param name="ask"></param>
        /// <param name="bid"></param>
        /// <param name="spread"></param>
        /// <param name="volume"></param>
        /// <param name="pnL"></param>
        /// <param name="startedAt"></param>
        /// <param name="endedAt"></param>
        public ArbitrageRow(AssetPair assetPair, string askSource, string bidSource, string askConversionPath, string bidConversionPath, VolumePrice ask, VolumePrice bid,
            decimal spread, decimal volume, decimal pnL, DateTime startedAt, DateTime endedAt)
        {
            AssetPair = assetPair;
            AskSource = string.IsNullOrWhiteSpace(askSource) ? throw new ArgumentNullException(nameof(askSource)) : askSource;
            BidSource = string.IsNullOrWhiteSpace(bidSource) ? throw new ArgumentNullException(nameof(bidSource)) : bidSource;
            AskConversionPath = string.IsNullOrWhiteSpace(askConversionPath) ? throw new ArgumentNullException(nameof(askConversionPath)) : askConversionPath;
            BidConversionPath = string.IsNullOrWhiteSpace(bidConversionPath) ? throw new ArgumentNullException(nameof(bidConversionPath)) : bidConversionPath;
            Ask = ask;
            Bid = bid;
            Spread = spread;
            Volume = volume;
            PnL = pnL;
            StartedAt = startedAt;
            EndedAt = endedAt;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
