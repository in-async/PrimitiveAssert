using System.Collections;

namespace Inasync {

    internal static class EnumerableExtensions {

        public static ICollection AsCollection(this IEnumerable source) {
            if (source is ICollection collection) { return collection; }

            var list = new ArrayList();
            foreach (var x in source) {
                list.Add(x);
            }
            return list;
        }
    }
}
