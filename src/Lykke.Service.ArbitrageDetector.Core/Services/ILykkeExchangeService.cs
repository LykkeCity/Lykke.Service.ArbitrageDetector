using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface ILykkeExchangeService
    {
        /// <summary>
        /// Try to infer assets in AssetPairStr.
        /// </summary>
        AssetPair InferBaseAndQuoteAssets(string assetPairStr);
    }
}
