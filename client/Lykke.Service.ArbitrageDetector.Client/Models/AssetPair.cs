namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an asset pair.
    /// </summary>
    public struct AssetPair
    {
        /// <summary>
        /// Base asset.
        /// </summary>
        public string Base{ get; set; }
        
        /// <summary>
        /// Quoting asset.
        /// </summary>
        public string Quoting { get; set; }
    }
}
