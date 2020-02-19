using System;

namespace Inasync {

    internal static class TypeExtensions {

        public static bool IsNullable(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsPrimitiveData(this Type type) {
            return type == typeof(bool)
                || type == typeof(char)
                || type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(Guid)
                || type == typeof(Uri)
                || type.IsEnum
                || type == typeof(sbyte)
                || type == typeof(byte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)
                || Nullable.GetUnderlyingType(type) is Type underingType && IsNumeric(underingType)
                ;
        }

        public static bool IsNumeric(this Type type) {
            return type == typeof(sbyte)
                || type == typeof(byte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)
                || Nullable.GetUnderlyingType(type) is Type underingType && IsNumeric(underingType)
                ;
        }
    }
}
