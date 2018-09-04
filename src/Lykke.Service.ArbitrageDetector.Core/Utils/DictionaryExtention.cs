using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Utils
{
    public static class DictionaryExtention
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var keyValue in other)
            {
                dictionary.Add(keyValue.Key, keyValue.Value);
            }
        }
    }
}
