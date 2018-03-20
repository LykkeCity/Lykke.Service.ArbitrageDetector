using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Arbitrage
    {
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
        public TimeSpan Lasted => EndedAt - StartedAt;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="askCrossRate"></param>
        /// <param name="ask"></param>
        /// <param name="bidCrossRate"></param>
        /// <param name="bid"></param>
        public Arbitrage(CrossRate askCrossRate, VolumePrice ask, CrossRate bidCrossRate, VolumePrice bid)
        {
            AskCrossRate = askCrossRate ?? throw new ArgumentNullException(nameof(askCrossRate));
            BidCrossRate = bidCrossRate ?? throw new ArgumentNullException(nameof(bidCrossRate));
            Ask = ask;
            Bid = bid;
            Spread = (Ask.Price - Bid.Price) / Bid.Price * 100;
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            PnL = Spread * Volume;
            StartedAt = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{AskCrossRate.AssetPair}, PnL: {Math.Round(PnL, 2)}, Spread: {Math.Round(Spread, 2)}%, Volume: {Math.Round(Volume, 2)}, Path: ({AskCrossRate.ConversionPath}) * ({BidCrossRate.ConversionPath})";
        }
    }
}
