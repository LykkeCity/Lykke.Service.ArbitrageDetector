using System;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Services.Models
{
    public sealed class CrossRateLine
    {
        public decimal Volume { get; set; }

        public decimal Price { get; set; }

        public CrossRate CrossRate { get; set; }

        public CrossRateLine(CrossRate crossRate, VolumePrice volumePrice)
        {
            Price = volumePrice.Price;
            Volume = volumePrice.Volume;
            CrossRate = crossRate ?? throw new ArgumentNullException(nameof(crossRate));
        }
    }
}
