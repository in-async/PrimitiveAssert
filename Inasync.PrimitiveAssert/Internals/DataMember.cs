using System;
using System.Reflection;

namespace Inasync {

    /// <summary>
    /// 型の get プロパティまたはフィールドを表します。
    /// </summary>
    internal readonly struct DataMember {
        private readonly PropertyInfo? _prop;
        private readonly FieldInfo? _field;

        public readonly string Name;
        public readonly Type DataType;
        public readonly Type DeclaringType;

        public DataMember(PropertyInfo prop) {
            _prop = prop;
            _field = null;
            Name = prop.Name;
            DataType = prop.PropertyType;
            DeclaringType = prop.DeclaringType;
        }

        public DataMember(FieldInfo field) {
            _prop = null;
            _field = field;
            Name = field.Name;
            DataType = field.FieldType;
            DeclaringType = field.DeclaringType;
        }

        public object GetValue(object? obj) {
            if (_prop != null) { return _prop.GetValue(obj); }
            return _field!.GetValue(obj);
        }
    }
}
