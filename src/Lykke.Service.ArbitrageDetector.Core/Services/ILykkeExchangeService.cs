using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface ILykkeExchangeService
    {
        /// <summary>
        /// Get price and volume accuracy for assetPair.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <returns></returns>
        (int Price, int Volume)? GetAccuracy(AssetPair assetPair);

        /// <summary>
        /// Try to infer assets in AssetPairStr.
        /// </summary>
        /// <param name="orderBook"></param>
        /// <returns>Number of infered assets. Can be 0, 1 or 2.</returns>
        AssetPair? InferBaseAndQuoteAssets(string assetPairStr);
    }
}
