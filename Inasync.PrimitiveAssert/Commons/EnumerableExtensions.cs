using System.Collections.Generic;

namespace Commons {

    internal static class EnumerableExtensions {

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) {
            return new HashSet<T>(source);
        }
    }
}
