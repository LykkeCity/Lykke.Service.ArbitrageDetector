using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using DomainOrderBook = Lykke.Service.ArbitrageDetector.Core.Domain.OrderBook;

namespace Lykke.Service.ArbitrageDetector.Models
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
        /// <param name="assetPair"></param>
        /// <param name="timestamp"></param>
        /// <param name="bids"></param>
        /// <param name="asks"></param>
        public OrderBook(string source, AssetPair assetPair, IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks, DateTime timestamp)
        {
            Source = string.IsNullOrWhiteSpace(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = assetPair;
            Bids = bids ?? throw new ArgumentNullException(nameof(bids));
            Asks = asks ?? throw new ArgumentNullException(nameof(asks));
            BestBid = Bids.Any() ? Bids.MaxBy(x => x.Price) : (VolumePrice?)null;
            BestAsk = Asks.Any() ? Asks.MinBy(x => x.Price) : (VolumePrice?)null;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain"></param>
        public OrderBook(DomainOrderBook domain)
            : this(domain.Source, new AssetPair(domain.AssetPair),
                domain.Bids.Select(x => new VolumePrice(x)).ToList(), domain.Asks.Select(x => new VolumePrice(x)).ToList(), domain.Timestamp)
        {
        }

        /// <summary>
        /// Returns new reversed order book.
        /// </summary>
        /// <returns></returns>
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
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <returns></returns>
        public static string FormatSourceAssetPair(string source, string assetPair)
        {
            return source + "-" + assetPair;
        }
    }
}
