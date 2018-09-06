using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static (decimal Volume, decimal PnL)? GetArbitrageVolumePnL(IEnumerable<VolumePrice> orderedBids, IEnumerable<VolumePrice> orderedAsks)
        {
            Debug.Assert(orderedBids != null);
            Debug.Assert(orderedAsks != null);

            var orderedBidsEnumerator = orderedBids.GetEnumerator();
            var orderedAsksEnumerator = orderedAsks.GetEnumerator();

            if (!orderedBidsEnumerator.MoveNext() || !orderedAsksEnumerator.MoveNext())
                return null;

            // Clone bids and asks
            decimal volume = 0;
            decimal pnl = 0;
            var bid = new VolumePrice(orderedBidsEnumerator.Current.Price, orderedBidsEnumerator.Current.Volume);
            var ask = new VolumePrice(orderedAsksEnumerator.Current.Price, orderedAsksEnumerator.Current.Volume);
            while (true)
            {
                if (bid.Price <= ask.Price)
                    break;

                var tradeBidPrice = bid.Price;
                var tradeAskPrice = ask.Price;
                if (bid.Volume < ask.Volume)
                {
                    volume += bid.Volume;
                    pnl += bid.Volume * (tradeBidPrice - tradeAskPrice);
                    ask.Volume = ask.Volume - bid.Volume;

                    if (!orderedBidsEnumerator.MoveNext()) break;
                    bid = orderedBidsEnumerator.Current;
                }
                else if (bid.Volume > ask.Volume)
                {
                    volume += ask.Volume;
                    pnl += ask.Volume * (tradeBidPrice - tradeAskPrice);
                    bid.Volume = bid.Volume - ask.Volume;

                    if (!orderedAsksEnumerator.MoveNext()) break;
                    ask = orderedAsksEnumerator.Current;
                }
                else if (bid.Volume == ask.Volume)
                {
                    volume += bid.Volume;
                    pnl += bid.Volume * (tradeBidPrice - tradeAskPrice);

                    if (!orderedBidsEnumerator.MoveNext()) break;
                    bid = orderedBidsEnumerator.Current;
                    if (!orderedAsksEnumerator.MoveNext()) break;
                    ask = orderedAsksEnumerator.Current;
                }
            }

            orderedBidsEnumerator.Dispose();
            orderedAsksEnumerator.Dispose();

            return volume == 0 ? ((decimal, decimal)?)null : (volume, Math.Round(pnl, 8));
        }
    }
}
