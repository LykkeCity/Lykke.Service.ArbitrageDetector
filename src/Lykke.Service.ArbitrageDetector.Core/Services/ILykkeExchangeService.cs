using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface ILykkeExchangeService
    {
        /// <summary>
        /// Get price and volume accuracy for assetPair.
        /// </summary>
        (int Price, int Volume)? GetAccuracy(AssetPair assetPair);

        /// <summary>
        /// Try to infer assets in AssetPairStr.
        /// </summary>
        AssetPair InferBaseAndQuoteAssets(string assetPairStr);
    }
}
