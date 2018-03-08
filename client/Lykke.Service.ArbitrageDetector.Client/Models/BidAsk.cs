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
        public decimal Bid { get; }

        /// <summary>
        /// Ask.
        /// </summary>
        public decimal Ask { get; }

        /// <summary>
        /// Contrsuctor.
        /// </summary>
        /// <param name="bid">Bid.</param>
        /// <param name="ask">Ask.</param>
        public BidAsk(decimal bid, decimal ask)
        {
            Bid = bid;
            Ask = ask;
        }
    }
}
