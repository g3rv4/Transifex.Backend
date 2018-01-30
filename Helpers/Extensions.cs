using System;
using System.Collections.Generic;

namespace Transifex.Backend.Helpers
{
    public static class Extensions
    {
        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dict, K key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            return default(V);
        }

        public static bool HasValue(this string str){
            return !String.IsNullOrEmpty(str);
        }
    }
}