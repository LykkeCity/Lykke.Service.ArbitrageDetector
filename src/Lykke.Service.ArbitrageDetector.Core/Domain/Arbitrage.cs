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
        /// Synthetic order book with high bid.
        /// </summary>
        public SynthOrderBook BidSynth { get; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; }

        /// <summary>
        /// Synthetic order book with low ask.
        /// </summary>
        public SynthOrderBook AskSynth { get; }

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
        public Arbitrage(AssetPair assetPair, SynthOrderBook bidSynth, VolumePrice bid, SynthOrderBook askSynth, VolumePrice ask)
        {
            AssetPair = assetPair;
            BidSynth = bidSynth ?? throw new ArgumentNullException(nameof(bidSynth));
            AskSynth = askSynth ?? throw new ArgumentNullException(nameof(askSynth));
            Bid = bid;
            Ask = ask;
            Spread = GetSpread(Bid.Price, Ask.Price);
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            PnL = GetPnL(Bid.Price, Ask.Price, Volume);
            ConversionPath = FormatConversionPath(BidSynth.ConversionPath, AskSynth.ConversionPath);
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
        public static string FormatConversionPath(string bidSynthOrderBookConversionPath, string askSynthOrderBookConversionPath)
        {
            return "(" + bidSynthOrderBookConversionPath + ") > (" + askSynthOrderBookConversionPath + ")";
        }

        /// <summary>
        /// Calculates spread.
        /// </summary>
        public static decimal GetSpread(decimal bidPrice, decimal askPrice)
        {
            return (askPrice - bidPrice) / bidPrice * 100;
        }

        /// <summary>
        /// Calculates PnL.
        /// </summary>
        public static decimal GetPnL(decimal bidPrice, decimal askPrice, decimal volume)
        {
            return (bidPrice - askPrice) * volume;
        }

        /// <summary>
        /// Calculates best volume (biggest spread strategy).
        /// </summary>
        public static (decimal? Volume, decimal? PnL)? GetArbitrageVolumePnL(IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks)
        {
            if (bids == null)
                throw new ArgumentException($"{nameof(bids)}");

            if (asks == null)
                throw new ArgumentException($"{nameof(asks)}");

            if (!bids.Any() || !asks.Any() || bids.Max(x => x.Price) <= asks.Min(x => x.Price))
                return null; // no arbitrage

            // Clone bids and asks (that in arbitrage only)
            var currentBids = new List<VolumePrice>();
            var currentAsks = new List<VolumePrice>();
            currentBids.AddRange(bids);
            currentAsks.AddRange(asks);

            decimal volume = 0;
            decimal pnl = 0;
            do
            {
                // Recalculate best bid and best ask
                var bestBidPrice = currentBids.Max(x => x.Price);
                var bestAskPrice = currentAsks.Min(x => x.Price);
                currentBids = currentBids.Where(x => x.Price > bestAskPrice).OrderByDescending(x => x.Price).ToList();
                currentAsks = currentAsks.Where(x => x.Price < bestBidPrice).OrderBy(x => x.Price).ToList();

                if (!currentBids.Any() || !currentAsks.Any()) // no more arbitrage
                    break;

                var bid = currentBids.First();
                var ask = currentAsks.First();

                // Calculate volume for current step and remove it
                decimal currentVolume = 0; 
                if (bid.Volume > ask.Volume)
                {
                    currentVolume = ask.Volume;
                    var newBidVolume = bid.Volume - ask.Volume;
                    var newBid = new VolumePrice(bid.Price, newBidVolume);
                    currentBids.Remove(bid);
                    currentBids.Insert(0, newBid);
                    currentAsks.Remove(ask);
                }

                if (bid.Volume < ask.Volume)
                {
                    currentVolume = bid.Volume;
                    var newAskVolume = ask.Volume - bid.Volume;
                    var newAsk = new VolumePrice(ask.Price, newAskVolume);
                    currentAsks.Remove(ask);
                    currentAsks.Insert(0, newAsk);
                    currentBids.Remove(bid);
                }

                if (bid.Volume == ask.Volume)
                {
                    currentVolume = bid.Volume;
                    currentBids.Remove(bid);
                    currentAsks.Remove(ask);
                }

                volume += currentVolume;
                pnl += currentVolume * (bid.Price - ask.Price);
            }
            while (currentBids.Any() && currentAsks.Any());

            return volume == 0 ? ((decimal?, decimal?)?)null : (volume, pnl);
        }


        /// <summary>
        /// Returns chained order books for arbitrage execution.
        /// </summary>
        public static IReadOnlyCollection<OrderBook> GetChainedOrderBooks(SynthOrderBook synthOrderBook, AssetPair target)
        {
            if (synthOrderBook == null)
                throw new NullReferenceException(nameof(synthOrderBook));

            var orderBooks = synthOrderBook.OriginalOrderBooks;

            var result = new List<OrderBook>();

            var @base = target.Base;   // BTC
            var quote = target.Quote;  // USD

            var first = orderBooks.Single(x => x.AssetPair.ContainsAsset(quote)); // Looking for USD|CHF
            if (first.AssetPair.Base == quote)  // Reverse if USD/CHF
                first = first.Reverse();
            result.Add(first);  // CHF/USD

            if (orderBooks.Count == 1)
                return result;

            var nextAsset = first.AssetPair.Base; // CHF
            var second = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAsset) && !x.AssetPair.IsEqualOrReversed(first.AssetPair)); // Looking for CHF|EUR
            if (second.AssetPair.Base == nextAsset)  // Reverse if CHF/EUR
                second = second.Reverse();
            result.Add(second);  // EUR/CHF

            if (orderBooks.Count == 2)
                if (second.AssetPair.Base != @base)
                    throw new InvalidOperationException($"{nameof(second)}.{nameof(second.AssetPair)}.{nameof(second.AssetPair.Base)}={second.AssetPair.Base} must be equal to {quote}");
                else
                    return result;

            nextAsset = second.AssetPair.Base; // EUR
            var third = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAsset) && x.AssetPair.ContainsAsset(@base)); // Looking for EUR|BTC
            if (third.AssetPair.Base == nextAsset)  // Reverse if EUR/BTC
                third = third.Reverse();
            result.Add(third);  // BTC/EUR

            return result;
        }
    }
}
