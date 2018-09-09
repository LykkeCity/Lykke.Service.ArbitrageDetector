using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class Arbitrage
    {
        public AssetPair AssetPair { get; }

        public SynthOrderBook BidSynth { get; }

        public VolumePrice Bid { get; }

        public SynthOrderBook AskSynth { get; }

        public VolumePrice Ask { get; }

        public decimal Spread { get; }
        
        public decimal Volume { get; }

        public decimal PnL { get; }

        public DateTime StartedAt { get; }

        public DateTime EndedAt { get; set; }

        public TimeSpan Lasted => EndedAt == default ? DateTime.UtcNow - StartedAt : EndedAt - StartedAt;

        public string ConversionPath { get; }

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

        public override string ToString()
        {
            return ConversionPath;
        }

        public static string FormatConversionPath(string bidSynthOrderBookConversionPath, string askSynthOrderBookConversionPath)
        {
            return "(" + bidSynthOrderBookConversionPath + ") > (" + askSynthOrderBookConversionPath + ")";
        }

        public static decimal GetSpread(decimal bidPrice, decimal askPrice)
        {
            return (askPrice - bidPrice) / bidPrice * 100;
        }

        public static decimal GetPnL(decimal bidPrice, decimal askPrice, decimal volume)
        {
            return (bidPrice - askPrice) * volume;
        }

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
                    ask.SubtractVolume(bid.Volume);

                    if (!orderedBidsEnumerator.MoveNext()) break;
                    bid = orderedBidsEnumerator.Current;
                }
                else if (bid.Volume > ask.Volume)
                {
                    volume += ask.Volume;
                    pnl += ask.Volume * (tradeBidPrice - tradeAskPrice);
                    bid.SubtractVolume(ask.Volume);

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
