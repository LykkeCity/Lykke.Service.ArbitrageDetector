namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a bid and an ask pair.
    /// </summary>
    public sealed class BidAsk
    {
        /// <summary>
        /// Bid.
        /// </summary>
        public decimal Bid { get; set; }

        /// <summary>
        /// Ask.
        /// </summary>
        public decimal Ask { get; set; }
    }
}
