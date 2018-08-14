using System;

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
        public decimal Price { get; }

        /// <summary>
        /// Volume.
        /// </summary>
        public decimal Volume { get; }

        /// <summary>
        /// Contructor.
        /// </summary>
        public VolumePrice(decimal price, decimal volume)
        {
            Price = price;
            Volume = Math.Abs(volume);
        }

        /// <summary>
        /// Returns reciprocal volume price.
        /// </summary>
        public VolumePrice Reciprocal()
        {
            return new VolumePrice(1 / Price, Volume * Price);
        }
    }
}
