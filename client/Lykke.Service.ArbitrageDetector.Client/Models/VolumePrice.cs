using System;

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
        public decimal Price { get; }
        
        /// <summary>
        /// Volume.
        /// </summary>
        public decimal Volume { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="price">Price.</param>
        /// <param name="volume">Volume.</param>
        public VolumePrice(decimal price, decimal volume)
        {
            Price = price;
            Volume = Math.Abs(volume);
        }
    }
}
