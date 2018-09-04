using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Utils
{
    public static class ConcurrentDictionaryExtention
    {
        public static void AddRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var keyValue in other)
            {
                if (dictionary.ContainsKey(keyValue.Key))
                    throw new InvalidOperationException("Dictionary already has that key.");

                dictionary[keyValue.Key] = keyValue.Value;
            }
        }

        public static void AddOrUpdateRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var keyValue in other)
            {
                dictionary[keyValue.Key] = keyValue.Value;
            }
        }

        public static void Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue oldValue;
            dictionary.Remove(key, out oldValue);
        }
    }
}
