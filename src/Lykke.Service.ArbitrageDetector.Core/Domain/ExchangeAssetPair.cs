using System;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct ExchangeAssetPair
    {
        [JsonProperty("exchange")]
        public string Exchange { get; }

        [JsonProperty("assetPair")]
        public string AssetPair{ get; }

        public ExchangeAssetPair(string exchange, string assetPair)
        {
            Exchange = string.IsNullOrEmpty(exchange) ? throw new ArgumentNullException(nameof(exchange)) : exchange;
            AssetPair = string.IsNullOrEmpty(assetPair) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
        }
    }
}
