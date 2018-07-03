using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IAssetsService
    {
        /// <summary>
        /// Get price and volume accuracy for assetPair.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <returns></returns>
        IAssetPairAccuracy GetAccuracy(AssetPair assetPair);

        /// <summary>
        /// Try to infer assets in AssetPairStr.
        /// </summary>
        /// <param name="orderBook"></param>
        /// <returns>Number of infered assets. Can be 0, 1 or 2.</returns>
        int InferBaseAndQuoteAssets(OrderBook orderBook);
    }
}
