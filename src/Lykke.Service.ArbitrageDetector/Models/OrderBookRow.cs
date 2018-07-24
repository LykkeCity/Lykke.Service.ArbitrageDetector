using System;
using System.Linq;
using DomainOrderBook = Lykke.Service.ArbitrageDetector.Core.Domain.OrderBook;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents a synthetic order book.
    /// </summary>
    public class OrderBookRow
    {
        /// <summary>
        /// Conversion path.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; }

        /// <summary>
        /// Best bid.
        /// </summary>
        public VolumePrice? BestBid { get; }

        /// <summary>
        /// Best ask.
        /// </summary>
        public VolumePrice? BestAsk { get; }

        /// <summary>
        /// Comulative volume of all bids.
        /// </summary>
        public decimal BidsVolume { get; }

        /// <summary>
        /// Comulative volume of all asks.
        /// </summary>
        public decimal AsksVolume { get; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <param name="bestBid"></param>
        /// <param name="bestAsk"></param>
        /// <param name="bidsVolume"></param>
        /// <param name="asksVolume"></param>
        /// <param name="timestamp"></param>
        public OrderBookRow(string source, AssetPair assetPair, VolumePrice? bestBid, VolumePrice? bestAsk, decimal bidsVolume, decimal asksVolume, DateTime timestamp)
        {
            Source = string.IsNullOrWhiteSpace(source) ? throw new ArgumentNullException(nameof(source)) : source;
            AssetPair = assetPair;
            BestBid = bestBid;
            BestAsk = bestAsk;
            BidsVolume = bidsVolume;
            AsksVolume = asksVolume;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain"></param>
        public OrderBookRow(DomainOrderBook domain)
        {
            Source = domain.Source;
            AssetPair = new AssetPair(domain.AssetPair);
            BestBid = VolumePrice.FromDomain(domain.BestBid);
            BestAsk = VolumePrice.FromDomain(domain.BestAsk);
            BidsVolume = domain.BidsVolume;
            AsksVolume = domain.AsksVolume;
            Timestamp = domain.Timestamp;
        }
    }
}
