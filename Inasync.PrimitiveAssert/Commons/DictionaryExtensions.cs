using System.Collections.Generic;

namespace Inasync {

    internal static class DictionaryExtensions {

        public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, out TValue value) {
            if (!source.TryGetValue(key, out value)) { return false; }
            source.Remove(key);
            return true;
        }
    }
}
