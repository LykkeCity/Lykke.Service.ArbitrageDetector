namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// represents an asset pair aka instrument.
    /// </summary>
    public struct AssetPair
    {
        /// <summary>
        /// Base asset.
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// Quote asset.
        /// </summary>
        public string Quote { get; set; }

        /// <summary>
        /// Name of the asset pair.
        /// </summary>
        public string Name => Base + Quote;

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
