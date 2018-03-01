using System;
using System.Collections.Generic;
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
            Asks = asks;
            Bids = bids;
            Timestamp = timestamp;
        }

        /// Must be refactored after addind base and quoting fields implementation in the Exchange Connector's OrderBook model
        public (string fromAsset, string toAsset)? GetAssetPairIfContains(string currency)
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

                return (fromAsset, toAsset);
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

    public sealed class VolumePrice
    {
        public VolumePrice(decimal price, decimal volume)
        {
            Price = price;
            Volume = Math.Abs(volume);
        }

        [JsonProperty("price")]
        public decimal Price { get; }

        [JsonProperty("volume")]
        public decimal Volume { get; }
    }
}
