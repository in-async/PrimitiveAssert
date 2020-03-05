using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons {

    internal static class TypeExtensions {

        /// <summary>
        /// <see cref="Nullable{T}"/> かどうか。
        /// </summary>
        public static bool IsNullable(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// <see cref="IEnumerable{T}"/> を実装するコレクション型から要素の型を返します。
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/7072088/why-does-type-getelementtype-return-null/7072121#7072121
        /// https://stackoverflow.com/questions/906499/getting-type-t-from-ienumerablet/17713382#17713382
        /// </remarks>
        public static Type? GetEnumerableElementType(this Type type) {
            if (type.IsArray) {
                return type.GetElementType();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                return type.GetGenericArguments()[0];
            }

            var enumerableType = type.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .FirstOrDefault();
            if (enumerableType != null) {
                return enumerableType.GetGenericArguments()[0];
            }

            return null;
        }
    }
}
