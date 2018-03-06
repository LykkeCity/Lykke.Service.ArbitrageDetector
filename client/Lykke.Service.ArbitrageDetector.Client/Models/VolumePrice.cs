namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a pair of a price and a volume.
    /// </summary>
    public sealed class VolumePrice
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
