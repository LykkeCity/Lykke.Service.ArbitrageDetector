using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class OrderBook
    {
        [JsonProperty("source")]
        public string Source { get; }

        [JsonProperty("asset")]
        public string AssetPairId { get; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; }

        [JsonProperty("asks")]
        public IReadOnlyCollection<VolumePrice> Asks { get; }

        [JsonProperty("bids")]
        public IReadOnlyCollection<VolumePrice> Bids { get; }

        public OrderBook(string source, string assetPairId, IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks, DateTime timestamp)
        {
            Source = source;
            AssetPairId = assetPairId;
            Asks = asks.OrderBy(x => x.Price).ToList();
            Bids = bids.OrderByDescending(x => x.Price).ToList();
            Timestamp = timestamp;
        }

        /// Must be removed after adding AssetsPairService
        public AssetPair? GetAssetPairIfContains(string currency)
        {
            var orderBookAssetPair = AssetPairId.ToUpper();
            if (orderBookAssetPair.Contains(currency))
            {
                var fromAsset = string.Empty;
                var toAsset = string.Empty;
                if (orderBookAssetPair.StartsWith(currency))
                {
                    fromAsset = currency;
                    toAsset = orderBookAssetPair.Replace(fromAsset, string.Empty);
                }
                else
                {
                    fromAsset = orderBookAssetPair.Replace(currency, string.Empty);
                    toAsset = currency;
                }

                return new AssetPair(fromAsset, toAsset);
            }

            return null;
        }

        public decimal GetBestBid()
        {
            return Bids.MaxBy(x => x.Price).Price;
        }

        public decimal GetBestAsk()
        {
            return Asks.MinBy(x => x.Price).Price;
        }
    }
}
