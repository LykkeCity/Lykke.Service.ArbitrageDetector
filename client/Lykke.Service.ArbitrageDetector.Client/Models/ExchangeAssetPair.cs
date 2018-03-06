namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a pair of exchange and an asset pair.
    /// </summary>
    public struct ExchangeAssetPair
    {
        /// <summary>
        /// A name of exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// An asset pair.
        /// </summary>
        public string AssetPair{ get; set; }
    }
}
