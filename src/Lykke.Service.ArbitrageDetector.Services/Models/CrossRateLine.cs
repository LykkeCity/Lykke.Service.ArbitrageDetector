using System;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Services.Models
{
    public sealed class SynthOrderBookLine
    {
        public decimal Volume { get; set; }

        public decimal Price { get; set; }

        public SynthOrderBook SynthOrderBook { get; set; }

        public SynthOrderBookLine(SynthOrderBook synthOrderBook, VolumePrice volumePrice)
        {
            Price = volumePrice.Price;
            Volume = volumePrice.Volume;
            SynthOrderBook = synthOrderBook ?? throw new ArgumentNullException(nameof(synthOrderBook));
        }
    }
}
