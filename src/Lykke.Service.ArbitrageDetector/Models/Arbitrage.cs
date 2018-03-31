using System;
using DomainArbitrage = Lykke.Service.ArbitrageDetector.Core.Domain.Arbitrage;

namespace Lykke.Service.ArbitrageDetector.Models
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
        public string ConversionPath => $"({AskCrossRate.ConversionPath}) * ({BidCrossRate.ConversionPath})";

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
            Spread = (Ask.Price - Bid.Price) / Bid.Price * 100;
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            PnL = Math.Abs(Spread * Volume);
            StartedAt = DateTime.UtcNow;
        }

        public Arbitrage(DomainArbitrage domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            if (domain.AskCrossRate == null)
                throw new ArgumentNullException(nameof(domain.AskCrossRate));

            if (domain.BidCrossRate == null)
                throw new ArgumentNullException(nameof(domain.BidCrossRate));

            AssetPair = new AssetPair(domain.AssetPair);
            AskCrossRate = new CrossRate(domain.AskCrossRate); 
            BidCrossRate = new CrossRate(domain.BidCrossRate);
            Ask = new VolumePrice(domain.Ask);
            Bid = new VolumePrice(domain.Bid);
            Spread = (Ask.Price - Bid.Price) / Bid.Price * 100;
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            PnL = Math.Abs(Spread * Volume);
            StartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// ToString() implementation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{AssetPair}-{ConversionPath}-{Ask.Price}-{Ask.Volume}-{Bid.Price}-{Bid.Volume}";
        }
    }
}
