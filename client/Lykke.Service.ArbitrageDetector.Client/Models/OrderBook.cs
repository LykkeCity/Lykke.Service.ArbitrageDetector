using System;
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
        /// Asking prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Asks { get; }

        /// <summary>
        /// Bidding prices and volumes.
        /// </summary>
        public IReadOnlyCollection<VolumePrice> Bids { get; }

        /// <summary>
        /// Best ask.
        /// </summary>
        public VolumePrice? BestAsk { get; }

        /// <summary>
        /// Best bid.
        /// </summary>
        public VolumePrice? BestBid { get; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <param name="asks"></param>
        /// <param name="bids"></param>
        /// <param name="timestamp"></param>
        public OrderBook(string source, AssetPair assetPair, IReadOnlyCollection<VolumePrice> asks, IReadOnlyCollection<VolumePrice> bids, DateTime timestamp)
        {
            Source = string.IsNullOrWhiteSpace(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = assetPair;
            Asks = asks ?? throw new ArgumentNullException(nameof(asks));
            Bids = bids ?? throw new ArgumentNullException(nameof(bids));
            BestAsk = Asks.Any() ? Asks.MinBy(x => x.Price) : (VolumePrice?)null;
            BestBid = Bids.Any() ? Bids.MaxBy(x => x.Price) : (VolumePrice?)null;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Returns new reversed order book.
        /// </summary>
        /// <returns></returns>
        public OrderBook Reverse()
        {
            var result = new OrderBook(Source, AssetPair.Reverse(),
                Bids.Select(x => x.Reciprocal()).OrderByDescending(x => x.Price).ToList(),
                Asks.Select(x => x.Reciprocal()).OrderByDescending(x => x.Price).ToList(),
                Timestamp);
            result.AssetPair = AssetPair.Reverse();

            return result;
        }

        /// <summary>
        /// ToString implementation.
        /// </summary>
        /// <returns></returns>
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
