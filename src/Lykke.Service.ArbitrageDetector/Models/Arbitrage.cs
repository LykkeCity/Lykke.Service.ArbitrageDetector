using System;
using DomainArbitrage = Lykke.Service.ArbitrageDetector.Core.Domain.Arbitrage;

namespace Lykke.Service.ArbitrageDetector.Models
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

        [Obsolete]
        public CrossRate BidCrossRate { get; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; }

        /// <summary>
        /// Synthetic order book with low ask.
        /// </summary>
        public SynthOrderBook AskSynth { get; }

        [Obsolete]
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
        /// Conversion path.
        /// </summary>
        public string ConversionPath => FormatConversionPath(BidSynth.ConversionPath, AskSynth.ConversionPath);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="bidSynth"></param>
        /// <param name="bid"></param>
        /// <param name="askSynth"></param>
        /// <param name="ask"></param>
        /// <param name="startedAt"></param>
        /// <param name="endedAt"></param>
        public Arbitrage(AssetPair assetPair, SynthOrderBook bidSynth, VolumePrice bid, SynthOrderBook askSynth, VolumePrice ask, DateTime startedAt, DateTime endedAt)
        {
            AssetPair = assetPair;
            BidSynth = bidSynth ?? throw new ArgumentNullException(nameof(bidSynth));
            AskSynth = askSynth ?? throw new ArgumentNullException(nameof(askSynth));
            Bid = bid;
            Ask = ask;
            Spread = GetSpread(Bid.Price, Ask.Price);
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            PnL = GetPnL(Bid.Price, Ask.Price, Volume);
            StartedAt = startedAt;
            EndedAt = endedAt;

            BidCrossRate = new CrossRate(BidSynth);
            AskCrossRate = new CrossRate(BidSynth);
        }

        /// <summary>
        /// Constructor from domain model.
        /// </summary>
        /// <param name="domain"></param>
        public Arbitrage(DomainArbitrage domain)
        : this(new AssetPair(domain.AssetPair), new SynthOrderBook(domain.BidSynth), new VolumePrice(domain.Bid),
            new SynthOrderBook(domain.AskSynth), new VolumePrice(domain.Ask), domain.StartedAt, domain.EndedAt)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="bidSynthOrderBookConversionPath"></param>
        /// <param name="askSynthOrderBookConversionPath"></param>
        /// <returns></returns>
        public static string FormatConversionPath(string bidSynthOrderBookConversionPath, string askSynthOrderBookConversionPath)
        {
            return "(" + bidSynthOrderBookConversionPath + ") > (" + askSynthOrderBookConversionPath + ")";
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
    }
}
