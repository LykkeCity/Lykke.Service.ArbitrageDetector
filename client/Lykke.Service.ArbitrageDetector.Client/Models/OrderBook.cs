﻿using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Client.Models
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
        public OrderBook(string source, AssetPair assetPair, IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks, DateTime timestamp)
        {
            Source = string.IsNullOrWhiteSpace(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = assetPair;
            Bids = bids ?? throw new ArgumentNullException(nameof(bids));
            Asks = asks ?? throw new ArgumentNullException(nameof(asks));
            Timestamp = timestamp;
        }

        /// <summary>
        /// Returns new reversed order book.
        /// </summary>
        public OrderBook Reverse()
        {
            var result = new OrderBook(Source, AssetPair.Reverse(),
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
        public static string FormatSourceAssetPair(string source, string assetPair)
        {
            return source + "-" + assetPair;
        }
    }
}
