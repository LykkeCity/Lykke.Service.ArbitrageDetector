using System;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct Arbitrage
    {
        [JsonProperty("ask")]
        public CrossRate LowAsk{ get; }

        [JsonProperty("bid")]
        public CrossRate HighBid { get; }

        public Arbitrage(CrossRate lowAsk, CrossRate highBid)
        {
            LowAsk = lowAsk ?? throw new ArgumentNullException(nameof(lowAsk));
            HighBid = highBid ?? throw new ArgumentNullException(nameof(highBid));
        }
    }
}
