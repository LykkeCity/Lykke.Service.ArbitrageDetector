using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class BidAsk
    {
        [JsonProperty("bid")]
        public decimal Bid { get; }

        [JsonProperty("ask")]
        public decimal Ask { get; }

        public BidAsk(decimal bid, decimal ask)
        {
            Bid = bid;
            Ask = ask;
        }
    }
}
