using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ArbitrageDetector.RabbitMq.Models
{
    internal class OrderBook
    {
        public string Source { get; }

        [JsonProperty("asset")]
        public string AssetPairStr { get; }

        public IReadOnlyList<VolumePrice> Bids { get; }

        public IReadOnlyList<VolumePrice> Asks { get; }

        public DateTime Timestamp { get; }

        public OrderBook(string source, string asset, IReadOnlyList<VolumePrice> bids, IReadOnlyList<VolumePrice> asks, DateTime timestamp)
        {
            Source = string.IsNullOrEmpty(source) ? throw new ArgumentException(nameof(source)) : source;
            AssetPairStr = string.IsNullOrWhiteSpace(asset) ? throw new ArgumentException(nameof(asset)) : asset;
            Bids = bids.Where(x => x.Price != 0 && x.Volume != 0).OrderByDescending(x => x.Price).ToList();
            Asks = asks.Where(x => x.Price != 0 && x.Volume != 0).OrderBy(x => x.Price).ToList();
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"{Source}-{AssetPairStr}";
        }
    }
}
