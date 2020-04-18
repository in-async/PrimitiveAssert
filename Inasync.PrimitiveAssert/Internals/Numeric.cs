using System;

namespace Inasync {

    /// <summary>
    /// <see cref="PrimitiveAssert"/> における数値型を表します。
    /// </summary>
    internal readonly struct Numeric : IEquatable<Numeric> {

        public static bool IsNumeric(Type type) {
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
                || type == typeof(Numeric)
                || Nullable.GetUnderlyingType(type) is Type underingType && IsNumeric(underingType)
                ;
        }

        public static bool TryCreate(object value, out Numeric result) {
            if (!IsNumeric(value.GetType())) {
                result = default;
                return false;
            }

            result = new Numeric(value);
            return true;
        }

        private readonly object _value;

        private Numeric(object value) {
            _value = value;
        }

        public override string ToString() {
            if (_value is decimal valueD) { return valueD.ToString("0.############################"); }
            if (_value is null) { return ""; }

            return _value.ToString();
        }

        public override bool Equals(object? obj) {
            if (obj == null) { return false; }
            return TryCreate(obj, out var numeric) && Equals(numeric);
        }

        public bool Equals(Numeric other) {
            return ToString().Equals(other.ToString());
        }

        public override int GetHashCode() {
            return -1939223833 + ToString().GetHashCode();
        }
    }
}
