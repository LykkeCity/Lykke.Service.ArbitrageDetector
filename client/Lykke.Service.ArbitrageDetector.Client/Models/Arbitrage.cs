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
        public CrossRate LowAsk{ get; set; }

        /// <summary>
        /// Cross rate with high ask.
        /// </summary>
        public CrossRate HighBid { get; set; }
    }
}
