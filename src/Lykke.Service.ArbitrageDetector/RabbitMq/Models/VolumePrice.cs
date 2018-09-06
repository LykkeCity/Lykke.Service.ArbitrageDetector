using System;

namespace Lykke.Service.ArbitrageDetector.RabbitMq.Models
{
    internal class VolumePrice
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
