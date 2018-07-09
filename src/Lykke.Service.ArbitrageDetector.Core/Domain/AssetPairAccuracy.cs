using System;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class AssetPairAccuracy
    {
        public AssetPair AssetPair { get; }

        public int PriceAccuracy { get; }

        public int VolumeAccuracy { get; }

        public AssetPairAccuracy(AssetPair assetPair, int priceAccuracy, int volumeAccuracy)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentOutOfRangeException(nameof(assetPair));

            AssetPair = assetPair;
            PriceAccuracy = priceAccuracy;
            VolumeAccuracy = volumeAccuracy;
        }
    }
}
