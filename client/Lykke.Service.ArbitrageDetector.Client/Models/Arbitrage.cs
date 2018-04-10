using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
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
        /// Cross rete with high bid.
        /// </summary>
        public CrossRate BidCrossRate { get; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; }

        /// <summary>
        /// Cross rete with low ask.
        /// </summary>
        public CrossRate AskCrossRate { get; }

        /// <summary>
        /// Price and volume of low ask.
        /// </summary>
        public VolumePrice Ask { get; }

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
        public string ConversionPath => FormatConversionPath(BidCrossRate.ConversionPath, AskCrossRate.ConversionPath);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="bidCrossRate"></param>
        /// <param name="bid"></param>
        /// <param name="askCrossRate"></param>
        /// <param name="ask"></param>
        public Arbitrage(AssetPair assetPair, CrossRate bidCrossRate, VolumePrice bid, CrossRate askCrossRate, VolumePrice ask, DateTime startedAt, DateTime endedAt)
        {
            AssetPair = assetPair;
            BidCrossRate = bidCrossRate ?? throw new ArgumentNullException(nameof(bidCrossRate));
            AskCrossRate = askCrossRate ?? throw new ArgumentNullException(nameof(askCrossRate));
            Bid = bid;
            Ask = ask;
            Spread = GetSpread(Bid.Price, Ask.Price);
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            PnL = GetPnL(Bid.Price, Ask.Price, Volume);
            StartedAt = startedAt;
            EndedAt = endedAt;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="bidCrossRateConversionPath"></param>
        /// <param name="askCrossRateConversionPath"></param>
        /// <returns></returns>
        public static string FormatConversionPath(string bidCrossRateConversionPath, string askCrossRateConversionPath)
        {
            return "(" + bidCrossRateConversionPath + ") > (" + askCrossRateConversionPath + ")";
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
