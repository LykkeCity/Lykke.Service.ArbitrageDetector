using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    /// TODO: Return to getters only with a constructor.
    public sealed class Arbitrage
    {
        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; set; }

        /// <summary>
        /// Synthetic order book with high bid.
        /// </summary>
        public SynthOrderBook BidSynth { get; set; }

        [Obsolete]
        public CrossRate BidCrossRate { get; set; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; set; }

        /// <summary>
        /// Synthetic order book with low ask.
        /// </summary>
        public SynthOrderBook AskSynth { get; set; }

        [Obsolete]
        public CrossRate AskCrossRate { get; set; }

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
        public TimeSpan Lasted => EndedAt == default ? DateTime.UtcNow - StartedAt : EndedAt - StartedAt;

        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath => FormatConversionPath(BidSynth.ConversionPath, AskSynth.ConversionPath);

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="bidSynthOrderBookConversionPath"></param>
        /// <param name="askSynthOrderBookConversionPath"></param>
        /// <returns></returns>
        public static string FormatConversionPath(string bidSynthOrderBookConversionPath, string askSynthOrderBookConversionPath)
        {
            return "(" + bidSynthOrderBookConversionPath + ") > (" + askSynthOrderBookConversionPath + ")";
        }

        /// <summary>
        /// Calculates spread.
        /// </summary>
        /// <param name="bidPrice"></param>
        /// <param name="askPrice"></param>
        /// <returns></returns>
        public static decimal GetSpread(decimal bidPrice, decimal askPrice)
        {
            return (askPrice - bidPrice) / bidPrice * 100;
        }

        /// <summary>
        /// Calculates PnL.
        /// </summary>
        /// <param name="bidPrice"></param>
        /// <param name="askPrice"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static decimal GetPnL(decimal bidPrice, decimal askPrice, decimal volume)
        {
            return (bidPrice - askPrice) * volume;
        }
    }
}
