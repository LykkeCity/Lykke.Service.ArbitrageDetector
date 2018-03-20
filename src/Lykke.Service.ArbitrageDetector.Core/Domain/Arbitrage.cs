using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
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
        public DateTime StartedTimestamp { get; }

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
            StartedTimestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{AskCrossRate.AssetPair}, spread: {Spread}, volume: {Volume}, path: ({AskCrossRate.ConversionPath}) * ({BidCrossRate.ConversionPath})";
        }
    }
}
