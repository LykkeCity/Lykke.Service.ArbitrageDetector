﻿using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Utils
{
    public static class ConcurrentDictionaryExtention
    {
        public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.AddOrUpdate(key, value, (_key, oldValue) => value);
        }

        public static void Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue oldValue;
            dictionary.Remove(key, out oldValue);
        }
    }
}