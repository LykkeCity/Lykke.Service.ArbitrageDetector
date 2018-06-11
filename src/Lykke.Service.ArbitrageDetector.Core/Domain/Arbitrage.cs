using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Arbitrage
    {
        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; }

        /// <summary>
        /// Cross rete with high bid.
        /// </summary>
        public CrossRate BidCrossRate { get; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; }

        /// <summary>
        /// Cross rete with low ask.
        /// </summary>
        public CrossRate AskCrossRate { get; }

        /// <summary>
        /// Price and volume of low ask.
        /// </summary>
        public VolumePrice Ask { get; }

        /// <summary>
        /// Spread between ask and bid.
        /// </summary>
        public decimal Spread { get; }

        /// <summary>
        /// The smallest volume of ask or bid.
        /// </summary>
        public decimal Volume { get; }

        /// <summary>
        /// Potential profit or loss.
        /// </summary>
        public decimal PnL { get; }

        /// <summary>
        /// The time when it first appeared.
        /// </summary>
        public DateTime StartedAt { get; }

        /// <summary>
        /// The time when it disappeared.
        /// </summary>
        public DateTime EndedAt { get; set; }

        /// <summary>
        /// How log the arbitrage lasted.
        /// </summary>
        public TimeSpan Lasted => EndedAt == default ? DateTime.UtcNow - StartedAt : EndedAt - StartedAt;

        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="bidCrossRate"></param>
        /// <param name="bid"></param>
        /// <param name="askCrossRate"></param>
        /// <param name="ask"></param>
        public Arbitrage(AssetPair assetPair, CrossRate bidCrossRate, VolumePrice bid, CrossRate askCrossRate, VolumePrice ask)
        {
            AssetPair = assetPair;
            BidCrossRate = bidCrossRate ?? throw new ArgumentNullException(nameof(bidCrossRate));
            AskCrossRate = askCrossRate ?? throw new ArgumentNullException(nameof(askCrossRate));
            Bid = bid;
            Ask = ask;
            Spread = GetSpread(Bid.Price, Ask.Price);
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            PnL = GetPnL(Bid.Price, Ask.Price, Volume);
            ConversionPath = FormatConversionPath(BidCrossRate.ConversionPath, AskCrossRate.ConversionPath);
            StartedAt = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="bidCrossRateConversionPath"></param>
        /// <param name="askCrossRateConversionPath"></param>
        /// <returns></returns>
        public static string FormatConversionPath(string bidCrossRateConversionPath, string askCrossRateConversionPath)
        {
            return "(" + bidCrossRateConversionPath + ") > (" + askCrossRateConversionPath + ")";
        }

        /// <summary>
        /// Calculates spread.
        /// </summary>
        /// <param name="bidPrice"></param>
        /// <param name="askPrice"></param>
        /// <returns></returns>
        public static decimal GetSpread(decimal bidPrice, decimal askPrice)
        {
            return (askPrice - bidPrice) / bidPrice * 100;
        }

        /// <summary>
        /// Calculates PnL.
        /// </summary>
        /// <param name="bidPrice"></param>
        /// <param name="askPrice"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static decimal GetPnL(decimal bidPrice, decimal askPrice, decimal volume)
        {
            return (bidPrice - askPrice) * volume;
        }

        /// <summary>
        /// Calculates best volume (biggest spread strategy).
        /// </summary>
        /// <param name="bidsOrderBook">OrderBook with bids.</param>
        /// <param name="asksOrderBook">OrderBook with asks.</param>
        /// <returns></returns>
        public static decimal? GetArbitrageVolume(OrderBook bidsOrderBook, OrderBook asksOrderBook)
        {
            if (bidsOrderBook.Bids == null)
                throw new ArgumentException($"{nameof(bidsOrderBook)}.{nameof(bidsOrderBook.Bids)}");

            if (asksOrderBook.Asks == null)
                throw new ArgumentException($"{nameof(asksOrderBook)}.{nameof(asksOrderBook.Asks)}");

            // Clone bids and asks (that in arbitrage only)
            var bids = new List<VolumePrice>();
            var asks = new List<VolumePrice>();
            bids.AddRange(bidsOrderBook.Bids);
            asks.AddRange(asksOrderBook.Asks);

            if (!bidsOrderBook.Bids.Any() || !asksOrderBook.Asks.Any() || bids.Max(x => x.Price) < asks.Min(x => x.Price))
                return null; // no arbitrage

            decimal result = 0;
            do
            {
                // Recalculate arbitrage (best bid and best ask)
                var bestBidPrice = bids.Max(x => x.Price);
                var bestAskPrice = asks.Min(x => x.Price);
                bids = bids.Where(x => x.Price > bestAskPrice).OrderByDescending(x => x.Price).ToList();
                asks = asks.Where(x => x.Price < bestBidPrice).OrderBy(x => x.Price).ToList();

                var bid = bids.First();
                var ask = asks.First();

                if (bid.Volume > ask.Volume)
                {
                    result += ask.Volume;
                    var newBidVolume = bid.Volume - ask.Volume;
                    var newBid = new VolumePrice(bid.Price, newBidVolume);
                    bids.Remove(bid);
                    bids.Insert(0, newBid);
                    asks.Remove(ask);
                    continue;
                }

                if (bid.Volume < ask.Volume)
                {
                    result += bid.Volume;
                    var newAskVolume = ask.Volume - bid.Volume;
                    var newAsk = new VolumePrice(ask.Price, newAskVolume);
                    asks.Remove(ask);
                    asks.Insert(0, newAsk);
                    bids.Remove(bid);
                    continue;
                }

                if (bid.Volume == ask.Volume)
                {
                    result += bid.Volume;
                    bids.Remove(bid);
                    asks.Remove(ask);
                }
            }
            while (bids.Any() && asks.Any());

            return result == 0 ? (decimal?)null : result;
        }
    }
}
