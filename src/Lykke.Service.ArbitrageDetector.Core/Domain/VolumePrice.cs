﻿using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct VolumePrice
    {
        public decimal Price { get; }

        public decimal Volume { get; private set; }

        public VolumePrice(decimal price, decimal volume)
        {
            Price = price;
            Volume = Math.Abs(volume);
        }

        public void SubtractVolume(decimal volume)
        {
            Volume -= volume;
        }

        public VolumePrice Reciprocal()
        {
            return new VolumePrice(1 / Price, Volume * Price);
        }

        public override string ToString()
        {
            return $"{Price:0.#####}, {Volume:0.#####}";
        }
    }
}
