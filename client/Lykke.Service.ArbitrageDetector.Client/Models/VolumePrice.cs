namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a pair of price and volume.
    /// </summary>
    public struct VolumePrice
    {
        /// <summary>
        /// Price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Volume.
        /// </summary>
        public decimal Volume { get; set; }
    }
}
