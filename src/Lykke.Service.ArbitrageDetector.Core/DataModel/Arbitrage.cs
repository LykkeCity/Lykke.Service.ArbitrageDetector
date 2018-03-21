using System;
using DomainArbitrage = Lykke.Service.ArbitrageDetector.Core.Domain.Arbitrage;

namespace Lykke.Service.ArbitrageDetector.Core.DataModel
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Arbitrage
    {
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
        public TimeSpan Lasted => EndedAt - StartedAt;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ask"></param>
        /// <param name="bid"></param>
        /// <param name="spread"></param>
        /// <param name="volume"></param>
        /// <param name="pnL"></param>
        /// <param name="startedAt"></param>
        /// <param name="endedAt"></param>
        public Arbitrage(VolumePrice ask, VolumePrice bid, decimal spread, decimal volume, decimal pnL, DateTime startedAt, DateTime endedAt)
        {
            Ask = ask;
            Bid = bid;
            Spread = spread;
            Volume = volume;
            PnL = pnL;
            StartedAt = startedAt;
            EndedAt = endedAt;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain"></param>
        public Arbitrage(DomainArbitrage domain)
        {
            Ask = new VolumePrice(domain.Ask);
            Bid = new VolumePrice(domain.Bid);
            Spread = domain.Spread;
            Volume = domain.Volume;
            PnL = domain.PnL;
            StartedAt = domain.StartedAt;
            EndedAt = domain.EndedAt;
        }
    }
}
