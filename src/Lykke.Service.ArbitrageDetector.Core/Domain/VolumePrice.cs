using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct VolumePrice
    {
        public decimal Price { get; }

        public decimal Volume { get; }

        public VolumePrice(decimal price, decimal volume)
        {
            Price = price;
            Volume = Math.Abs(volume);
        }
    }
}
