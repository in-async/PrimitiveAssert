using System;

namespace Commons {

    internal static class TypeExtensions {

        /// <summary>
        /// <see cref="Nullable{T}"/> かどうか。
        /// </summary>
        public static bool IsNullable(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
