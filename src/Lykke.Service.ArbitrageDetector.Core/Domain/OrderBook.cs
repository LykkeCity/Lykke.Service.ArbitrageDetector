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
        /// Bidding prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Bids { get; protected set; }

        /// <summary>
        /// Asking prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Asks { get; protected set; }

        /// <summary>
        /// Best bid.
        /// </summary>
        public VolumePrice? BestBid => Bids.Any() ? Bids.MaxBy(x => x.Price) : (VolumePrice?)null;

        /// <summary>
        /// Best ask.
        /// </summary>
        public VolumePrice? BestAsk => Asks.Any() ? Asks.MinBy(x => x.Price) : (VolumePrice?)null;

        /// <summary>
        /// All bids volume.
        /// </summary>
        public decimal BidsVolume => Bids.Sum(x => x.Volume);

        /// <summary>
        /// All asks volume.
        /// </summary>
        public decimal AsksVolume => Asks.Sum(x => x.Volume);

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public OrderBook(string source, string asset, IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks, DateTime timestamp)
        {
            Source = string.IsNullOrEmpty(source) ? throw new ArgumentException(nameof(source)) : source;
            AssetPairStr = string.IsNullOrEmpty(asset) ? throw new ArgumentException(nameof(asset)) : asset;
            Bids = bids.Where(x => x.Price != 0 && x.Volume != 0)
                       .OrderByDescending(x => x.Price).ToList();
            Asks = asks.Where(x => x.Price != 0 && x.Volume != 0)
                       .OrderBy(x => x.Price).ToList();
            Timestamp = timestamp;
        }

        /// <summary>
        /// Set asset pair to AssetPair from string by providing onw of the asset.
        /// </summary>
        public void SetAssetPair(string oneOfTheAssets)
        {
            if (string.IsNullOrWhiteSpace(oneOfTheAssets))
                throw new ArgumentNullException(nameof(oneOfTheAssets));

            AssetPair = AssetPair.FromString(AssetPairStr, oneOfTheAssets);
        }

        /// <summary>
        /// Set AssetPair.
        /// </summary>
        public void SetAssetPair(AssetPair assetPair)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentNullException(nameof(assetPair));

            AssetPair = assetPair;
        }

        /// <summary>
        /// Returns new reversed order book.
        /// </summary>
        public OrderBook Reverse()
        {
            var result = new OrderBook(Source, AssetPair.Quote + AssetPair.Base,
                Asks.Select(x => x.Reciprocal()).OrderByDescending(x => x.Price).ToList(),
                Bids.Select(x => x.Reciprocal()).OrderByDescending(x => x.Price).ToList(),
                Timestamp);
            result.AssetPair = AssetPair.Reverse();

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FormatSourceAssetPair(Source, AssetPair.IsEmpty() ? AssetPairStr : AssetPair.Name);
        }

        /// <summary>
        /// Formats source asset pair.
        /// </summary>
        public static string FormatSourceAssetPair(string source, string assetPair)
        {
            return source + "-" + assetPair;
        }

        /// <summary>
        /// Returns a deep clone.
        /// </summary>
        public OrderBook DeepClone(decimal fee = 0)
        {
            var result = (OrderBook)MemberwiseClone();

            if (fee == 0)
            {
                Bids = new List<VolumePrice>(Bids);
                Asks = new List<VolumePrice>(Asks);
            }
            else
            {
                var bids = new List<VolumePrice>();
                foreach (var bid in Bids)
                {
                    var newVolumePrice = new VolumePrice(bid.Price - (bid.Price / 100 * fee), bid.Volume);
                    bids.Add(newVolumePrice);
                }
                result.Bids = bids;

                var asks = new List<VolumePrice>();
                foreach (var ask in Asks)
                {
                    var newVolumePrice = new VolumePrice(ask.Price + (ask.Price / 100 * fee), ask.Volume);
                    asks.Add(newVolumePrice);
                }
                result.Asks = asks;
            }

            return result;
        }
    }
}
