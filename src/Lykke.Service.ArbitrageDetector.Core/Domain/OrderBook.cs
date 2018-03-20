using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Newtonsoft.Json;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents an order book or depth of market.
    /// </summary>
    public class OrderBook
    {
        /// <summary>
        /// Exchange name.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// String of an asset pair.
        /// </summary>
        [JsonProperty("asset")]
        public string AssetPairStr { get; }

        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; set; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; protected set; }

        /// <summary>
        /// Asking prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Asks { get; }

        /// <summary>
        /// Bidding prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Bids { get; }

        public decimal BestBidPrice => Bids.MaxBy(x => x.Price).Price;
        public decimal BestBidVolume => Bids.MaxBy(x => x.Price).Volume;
        public decimal BestAskPrice => Asks.MinBy(x => x.Price).Price;
        public decimal BestAskVolume => Asks.MinBy(x => x.Price).Volume;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="asset"></param>
        /// <param name="asks"></param>
        /// <param name="bids"></param>
        /// <param name="timestamp"></param>
        public OrderBook(string source, string asset, IReadOnlyCollection<VolumePrice> asks, IReadOnlyCollection<VolumePrice> bids, DateTime timestamp)
        {
            Source = string.IsNullOrEmpty(source) ? throw new ArgumentException(nameof(source)) : source;
            AssetPairStr = string.IsNullOrEmpty(asset) ? throw new ArgumentException(nameof(asset)) : asset;
            Asks = asks.OrderBy(x => x.Price).ToList();
            Bids = bids.OrderByDescending(x => x.Price).ToList();
            Timestamp = timestamp;
        }

        /// <summary>
        /// Set asset pair to AssetPair from string by providing onw of the asset.
        /// </summary>
        /// <param name="oneOfTheAssets"></param>
        public void SetAssetPair(string oneOfTheAssets)
        {
            if (string.IsNullOrWhiteSpace(oneOfTheAssets))
                throw new ArgumentNullException(nameof(oneOfTheAssets));

            AssetPair = AssetPair.FromString(AssetPairStr, oneOfTheAssets);
        }

        /// <summary>
        /// Returns new reversed order book.
        /// </summary>
        /// <returns></returns>
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
