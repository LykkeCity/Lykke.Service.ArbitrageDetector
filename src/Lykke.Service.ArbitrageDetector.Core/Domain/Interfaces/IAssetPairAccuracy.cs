namespace Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces
{
    public interface IAssetPairAccuracy
    {
        AssetPair AssetPair { get; }

        int PriceAccuracy { get; }

        int VolumeAccuracy { get; }
    }
}
