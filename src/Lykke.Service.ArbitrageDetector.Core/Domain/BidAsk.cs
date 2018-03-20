namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents ask and bid prices.
    /// </summary>
    public struct BidAsk
    {
        /// <summary>
        /// Ask.
        /// </summary>
        public decimal Ask { get; }

        /// <summary>
        /// Bid.
        /// </summary>
        public decimal Bid { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="ask"></param>
        public BidAsk(decimal bid, decimal ask)
        {
            Bid = bid;
            Ask = ask;
        }
    }
}
