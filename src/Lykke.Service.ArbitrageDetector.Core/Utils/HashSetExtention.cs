using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Utils
{
    public static class HashSetExtention
    {
        public static void AddOrUpdate<TValue>(this HashSet<TValue> hashSet, TValue value)
        {
            if (value == null)
                throw new NullReferenceException($"intermediateWantedCrossRate is null");

            if (hashSet.Contains(value))
                hashSet.Remove(value);

            hashSet.Add(value);
        }
    }
}
