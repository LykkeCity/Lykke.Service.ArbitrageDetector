using System;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct AssetPair
    {
        [JsonProperty("base")]
        public string Base{ get; }

        [JsonProperty("quoting")]
        public string Quoting { get; }

        public AssetPair(string _base, string quoting)
        {
            Base = string.IsNullOrEmpty(_base) ? throw new ArgumentNullException(nameof(_base)) : _base;
            Quoting = string.IsNullOrEmpty(quoting) ? throw new ArgumentNullException(nameof(quoting)) : quoting;
        }
    }
}
