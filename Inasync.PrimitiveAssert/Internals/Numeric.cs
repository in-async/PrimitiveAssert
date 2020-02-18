using System;

namespace Inasync {

    internal readonly struct Numeric : IEquatable<Numeric> {

        public static bool TryCreate(object value, out Numeric result) {
            if (!value.GetType().IsNumeric()) {
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
            return obj is Numeric numeric && Equals(numeric);
        }

        public bool Equals(Numeric other) {
            return ToString().Equals(other.ToString());
        }

        public override int GetHashCode() {
            return -1939223833 + ToString().GetHashCode();
        }
    }
}
