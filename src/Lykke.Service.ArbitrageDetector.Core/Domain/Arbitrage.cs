using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Arbitrage
    {
        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; }

        /// <summary>
        /// Cross rete with low ask.
        /// </summary>
        public CrossRate AskCrossRate { get; }

        /// <summary>
        /// Price and volume of low ask.
        /// </summary>
        public VolumePrice Ask { get; }

        /// <summary>
        /// Cross rete with high bid.
        /// </summary>
        public CrossRate BidCrossRate { get; }

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
        public string ConversionPath { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="askCrossRate"></param>
        /// <param name="ask"></param>
        /// <param name="bidCrossRate"></param>
        /// <param name="bid"></param>
        public Arbitrage(AssetPair assetPair, CrossRate askCrossRate, VolumePrice ask, CrossRate bidCrossRate, VolumePrice bid)
        {
            AssetPair = assetPair;
            AskCrossRate = askCrossRate ?? throw new ArgumentNullException(nameof(askCrossRate));
            BidCrossRate = bidCrossRate ?? throw new ArgumentNullException(nameof(bidCrossRate));
            Ask = ask;
            Bid = bid;
            Spread = GetSpread(Ask.Price, Bid.Price);
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            PnL = GetPnL(Ask.Price, Bid.Price, Volume);
            ConversionPath = FormatConversionPath(AskCrossRate.ConversionPath, BidCrossRate.ConversionPath);
            StartedAt = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="askCrossRateConversionPath"></param>
        /// <param name="bidCrossRateConversionPath"></param>
        /// <returns></returns>
        public static string FormatConversionPath(string askCrossRateConversionPath, string bidCrossRateConversionPath)
        {
            return "(" + askCrossRateConversionPath + ") < (" + bidCrossRateConversionPath + ")";
        }

        /// <summary>
        /// Calculates spread.
        /// </summary>
        /// <param name="askPrice"></param>
        /// <param name="bidPrice"></param>
        /// <returns></returns>
        public static decimal GetSpread(decimal askPrice, decimal bidPrice)
        {
            return (askPrice - bidPrice) / bidPrice * 100;
        }

        /// <summary>
        /// Calculates PnL.
        /// </summary>
        /// <param name="askPrice"></param>
        /// <param name="bidPrice"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static decimal GetPnL(decimal askPrice, decimal bidPrice, decimal volume)
        {
            return (bidPrice - askPrice) * volume;
        }
    }
}
