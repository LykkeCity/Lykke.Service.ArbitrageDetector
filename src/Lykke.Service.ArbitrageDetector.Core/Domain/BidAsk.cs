namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct BidAsk
    {
        public decimal Bid { get; }

        public decimal Ask { get; }

        public BidAsk(decimal bid, decimal ask)
        {
            Bid = bid;
            Ask = ask;
        }
    }
}
