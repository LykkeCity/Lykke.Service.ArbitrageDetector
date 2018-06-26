﻿using System;
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
        public IReadOnlyCollection<VolumePrice> Bids { get; }

        /// <summary>
        /// Asking prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Asks { get; }

        /// <summary>
        /// Best bid.
        /// </summary>
        public VolumePrice? BestBid { get; }

        /// <summary>
        /// Best ask.
        /// </summary>
        public VolumePrice? BestAsk { get; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="asset"></param>
        /// <param name="bids"></param>
        /// <param name="asks"></param>
        /// <param name="timestamp"></param>
        public OrderBook(string source, string asset, IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks, DateTime timestamp)
        {
            Source = string.IsNullOrEmpty(source) ? throw new ArgumentException(nameof(source)) : source;
            AssetPairStr = string.IsNullOrEmpty(asset) ? throw new ArgumentException(nameof(asset)) : asset;
            Bids = bids.OrderByDescending(x => x.Price).ToList();
            Asks = asks.OrderBy(x => x.Price).ToList();
            BestBid = Bids.Any() ? Bids.MaxBy(x => x.Price) : (VolumePrice?)null;
            BestAsk = Asks.Any() ? Asks.MinBy(x => x.Price) : (VolumePrice?)null;
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
        /// Set AssetPair.
        /// </summary>
        /// <param name="assetPair"></param>
        public void SetAssetPair(AssetPair assetPair)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentNullException(nameof(assetPair));

            AssetPair = assetPair;
        }

        /// <summary>
        /// Returns new reversed order book.
        /// </summary>
        /// <returns></returns>
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
            return FormatSourceAssetPair(Source, AssetPair.Name);
        }

        /// <summary>
        /// Formats source asset pair.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <returns></returns>
        public static string FormatSourceAssetPair(string source, string assetPair)
        {
            return source + "-" + assetPair;
        }
    }
}
