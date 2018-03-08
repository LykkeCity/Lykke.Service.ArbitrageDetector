using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a pair of exchange and an asset pair.
    /// </summary>
    public struct ExchangeAssetPair
    {
        /// <summary>
        /// Name of exchange.
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Asset pair.
        /// </summary>
        public string AssetPair{ get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exchange">Exchange name.</param>
        /// <param name="assetPair">Asset pair.</param>
        public ExchangeAssetPair(string exchange, string assetPair)
        {
            Exchange = string.IsNullOrEmpty(exchange) ? throw new ArgumentNullException(nameof(exchange)) : exchange;
            AssetPair = string.IsNullOrEmpty(assetPair) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
        }
    }
}
