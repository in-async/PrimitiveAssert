using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        /// <summary>
        /// 指定された型が備えるプロパティを全て返します。
        /// <paramref name="type"/> がインターフェースの場合は、そのインターフェースが実装する全てのインターフェースのプロパティも含みます。
        /// </summary>
        /// <remarks>https://stackoverflow.com/questions/358835/getproperties-to-return-all-properties-for-an-interface-inheritance-hierarchy/26766221#26766221</remarks>
        public static IEnumerable<PropertyInfo> GetPropertiesEx(this Type type, BindingFlags bindingAttr) {
            if (!type.IsInterface) {
                return type.GetProperties(bindingAttr);
            }

            return new[] { type }
                   .Concat(type.GetInterfaces())
                   .SelectMany(x => x.GetProperties(bindingAttr))
                   ;
        }
    }
}
