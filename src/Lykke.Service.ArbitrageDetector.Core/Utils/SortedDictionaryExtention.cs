using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Utils
{
    public static class SortedDictionaryExtention
    {
        public static void AddOrUpdate<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary.Remove(key);

            dictionary.Add(key, value);
        }
    }
}
