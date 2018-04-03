using System;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Services.Models
{
    public sealed class ArbitrageLine
    {
        public decimal Volume { get; set; }

        public decimal Price { get; set; }

        public CrossRate CrossRate { get; set; }

        public ArbitrageLine(CrossRate crossRate, VolumePrice volumePrice)
        {
            Price = volumePrice.Price;
            Volume = volumePrice.Volume;
            CrossRate = crossRate ?? throw new ArgumentNullException(nameof(crossRate));
        }
    }
}
