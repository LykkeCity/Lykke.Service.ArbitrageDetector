using System;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct AssetPair
    {
        [JsonProperty("base")]
        public string Base{ get; }

        [JsonProperty("quote")]
        public string Quote { get; }

        public AssetPair(string _base, string quote)
        {
            Base = string.IsNullOrEmpty(_base) ? throw new ArgumentNullException(nameof(_base)) : _base;
            Quote = string.IsNullOrEmpty(quote) ? throw new ArgumentNullException(nameof(quote)) : quote;
        }
    }
}
