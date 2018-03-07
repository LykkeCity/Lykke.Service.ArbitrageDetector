using System;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct Arbitrage
    {
        [JsonProperty("lowAsk")]
        public CrossRate LowAsk{ get; }

        [JsonProperty("highBid")]
        public CrossRate HighBid { get; }

        public Arbitrage(CrossRate lowAsk, CrossRate highBid)
        {
            LowAsk = lowAsk ?? throw new ArgumentNullException(nameof(lowAsk));
            HighBid = highBid ?? throw new ArgumentNullException(nameof(highBid));
        }
    }
}
