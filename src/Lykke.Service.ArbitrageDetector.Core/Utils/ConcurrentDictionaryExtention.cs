using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Utils
{
    public static class ConcurrentDictionaryExtention
    {
        public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.AddOrUpdate(key, value, (_key, oldValue) => value);
        }

        public static void Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            dictionary.AddOrUpdate(key, value);
        }

        public static void AddRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, ConcurrentDictionary<TKey, TValue> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var keyValue in other)
            {
                dictionary.Add(keyValue.Key, keyValue.Value);
            }
        }

        public static void AddOrUpdateRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, ConcurrentDictionary<TKey, TValue> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var keyValue in other)
            {
                dictionary.AddOrUpdate(keyValue.Key, keyValue.Value);
            }
        }

        public static void Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue oldValue;
            dictionary.Remove(key, out oldValue);
        }
    }
}
