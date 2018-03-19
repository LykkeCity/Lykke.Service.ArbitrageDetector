using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class Arbitrage
    {
        public CrossRate AskCrossRate { get; }

        public VolumePrice Ask { get; }


        public CrossRate BidCrossRate { get; }

        public VolumePrice Bid { get; }


        public decimal Spread { get; }

        public decimal Volume { get; }


        public DateTime StartedTimestamp { get; }


        public Arbitrage(CrossRate askCrossRate, VolumePrice ask, CrossRate bidCrossRate, VolumePrice bid)
        {
            AskCrossRate = askCrossRate ?? throw new ArgumentNullException(nameof(askCrossRate));
            BidCrossRate = bidCrossRate ?? throw new ArgumentNullException(nameof(bidCrossRate));
            Ask = ask;
            Bid = bid;
            Spread = (Ask.Price - Bid.Price) / Bid.Price * 100;
            Volume = Ask.Volume < Bid.Volume ? Ask.Volume : Bid.Volume;
            StartedTimestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{AskCrossRate.AssetPair}, spread: {Spread}, volume: {Volume}, path: ({AskCrossRate.ConversionPath}) * ({BidCrossRate.ConversionPath})";
        }
    }
}
