using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class LykkeArbitrageRow
    {
        public AssetPair Target { get; }

        public AssetPair Source { get; }

        public int SourcesCount { get; set; }

        public int SynthsCount { get; set; }

        public decimal Spread { get; }

        public string TargetSide { get; }

        public string ConversionPath { get; }

        public decimal Volume { get; }

        public decimal? VolumeInUsd { get; }

        public decimal PnL { get; }

        public decimal? PnLInUsd { get; }

        public decimal? BaseAsk { get; }

        public decimal? BaseBid { get; }

        public decimal? SynthAsk { get; }

        public decimal? SynthBid { get; }

        public LykkeArbitrageRow(AssetPair baseAssetPair, AssetPair crossAssetPair, decimal spread, string baseSide,
            string conversionPath, decimal volume, decimal? baseBid, decimal? baseAsk, decimal? synthBid, decimal? synthAsk, decimal? volumeInUsd,
            decimal pnL, decimal? pnLInUsd)
        {
            Target = baseAssetPair ?? throw new ArgumentNullException(nameof(baseAssetPair));
            Source = crossAssetPair ?? throw new ArgumentNullException(nameof(crossAssetPair));
            Spread = spread;
            TargetSide = string.IsNullOrWhiteSpace(baseSide) ? throw new ArgumentNullException(nameof(baseSide)) : baseSide;
            ConversionPath = string.IsNullOrWhiteSpace(conversionPath) ? throw new ArgumentNullException(nameof(conversionPath)) : conversionPath;
            Volume = volume;
            BaseAsk = baseAsk;
            BaseBid = baseBid;
            SynthAsk = synthAsk;
            SynthBid = synthBid;
            VolumeInUsd = volumeInUsd;
            PnL = pnL;
            PnLInUsd = pnLInUsd;
        }

        public override string ToString()
        {
            return Target + "-" + Source + " : " + ConversionPath;
        }
    }
}
