using System;

namespace Lykke.Service.ArbitrageDetector.Aspects.Cache
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CacheAttribute : Attribute
    {
        public int Duration { get; set; }
    }
}
