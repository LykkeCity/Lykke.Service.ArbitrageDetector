namespace Lykke.Service.ArbitrageDetector.Client.Models
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
