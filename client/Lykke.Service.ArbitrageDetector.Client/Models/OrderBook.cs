using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    public class OrderBook
    {
        public string Source { get; }

        [JsonProperty("asset")]
        public string AssetPairStr { get; }

        public AssetPair AssetPair { get; set; }

        public DateTime Timestamp { get; protected set; }

        public IReadOnlyCollection<VolumePrice> Asks { get; }

        public IReadOnlyCollection<VolumePrice> Bids { get; }

        public decimal BestBidPrice => Bids.MaxBy(x => x.Price).Price;
        public decimal BestBidVolume => Bids.MaxBy(x => x.Price).Volume;
        public decimal BestAskPrice => Asks.MinBy(x => x.Price).Price;
        public decimal BestAskVolume => Asks.MinBy(x => x.Price).Volume;

        public OrderBook(string source, string asset, IReadOnlyCollection<VolumePrice> asks, IReadOnlyCollection<VolumePrice> bids, DateTime timestamp)
        {
            Source = string.IsNullOrEmpty(source) ? throw new ArgumentException(nameof(source)) : source;
            AssetPairStr = string.IsNullOrEmpty(asset) ? throw new ArgumentException(nameof(asset)) : asset;
            Asks = asks.OrderBy(x => x.Price).ToList();
            Bids = bids.OrderByDescending(x => x.Price).ToList();
            Timestamp = timestamp;
        }

        public void SetAssetPair(string oneOfTheAssets)
        {
            if (string.IsNullOrWhiteSpace(oneOfTheAssets))
                throw new ArgumentNullException(nameof(oneOfTheAssets));

            AssetPair = AssetPair.FromString(AssetPairStr, oneOfTheAssets);
        }

        public OrderBook Reverse()
        {
            var inversedAssetPair = AssetPair.Quoting + AssetPair.Base;
            var result = new OrderBook(Source, inversedAssetPair,
                Bids.Select(x => new VolumePrice(1 / x.Price, x.Volume)).OrderByDescending(x => x.Price).ToList(),
                Asks.Select(x => new VolumePrice(1 / x.Price, x.Volume)).OrderByDescending(x => x.Price).ToList(),
                Timestamp);
            result.AssetPair = new AssetPair(AssetPair.Quoting, AssetPair.Base);

            return result;
        }
    }
}
