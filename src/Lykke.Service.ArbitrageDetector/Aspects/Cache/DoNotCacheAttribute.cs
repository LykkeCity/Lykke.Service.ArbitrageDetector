using System;

namespace Lykke.Service.ArbitrageDetector.Aspects.Cache
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class DoNotCacheAttribute : Attribute
    {
        public int Duration { get; set; }
    }
}
