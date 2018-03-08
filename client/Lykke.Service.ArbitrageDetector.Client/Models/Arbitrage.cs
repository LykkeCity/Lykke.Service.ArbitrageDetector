using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public struct Arbitrage
    {
        /// <summary>
        /// Cross rate with low ask.
        /// </summary>
        public CrossRate LowAsk{ get; }

        /// <summary>
        /// Cross rate with high ask.
        /// </summary>
        public CrossRate HighBid { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lowAsk">Low ask.</param>
        /// <param name="highBid">High bid.</param>
        public Arbitrage(CrossRate lowAsk, CrossRate highBid)
        {
            LowAsk = lowAsk ?? throw new ArgumentNullException(nameof(lowAsk));
            HighBid = highBid ?? throw new ArgumentNullException(nameof(highBid));
        }
    }
}
