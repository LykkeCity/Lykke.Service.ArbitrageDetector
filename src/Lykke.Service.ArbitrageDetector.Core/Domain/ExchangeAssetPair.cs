using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct ExchangeAssetPair
    {
        public string Exchange { get; }
        
        public string AssetPair{ get; }

        public ExchangeAssetPair(string exchange, string assetPair)
        {
            Exchange = string.IsNullOrEmpty(exchange) ? throw new ArgumentNullException(nameof(exchange)) : exchange;
            AssetPair = string.IsNullOrEmpty(assetPair) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
        }
    }
}
