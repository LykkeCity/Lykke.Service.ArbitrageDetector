using System;

namespace Lykke.Service.ArbitrageDetector.Aspects.Cache
{
    public class CacheAttribute : Attribute
    {
        public int Duration { get; set; }
    }
}
