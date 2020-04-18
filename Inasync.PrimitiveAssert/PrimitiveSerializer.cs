using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Commons;

namespace Inasync {

    public static class PrimitiveSerializerExtensions {

        public static string ToPrimitiveString(this object? obj, bool indented = false) {
            return PrimitiveSerializer.Serialize(obj, indented);
        }
    }

    public sealed class PrimitiveSerializer {

        public static string Serialize(object? obj, bool indented = false) {
            using var writer = new StringWriter();
            Serialize(writer, obj, indented);
            writer.Flush();
            return writer.ToString();
        }

        public static void Serialize(TextWriter writer, object? obj, bool indented = false) {
            if (writer is null) { throw new ArgumentNullException(nameof(writer)); }

            using var indentedWriter = new IndentedTextWriter(writer, tabString: indented ? "    " : "");
            if (!indented) {
                indentedWriter.NewLine = " ";
            }

            new PrimitiveSerializer(indentedWriter, memberSpace: true).Invoke(obj);
        }

        private readonly IndentedTextWriter _writer;
        private readonly bool _memberSpace;
        private readonly HashSet<object> _refInstances = new HashSet<object>();

        private PrimitiveSerializer(IndentedTextWriter writer, bool memberSpace) {
            _writer = writer;
            _memberSpace = memberSpace;
        }

        private void Invoke(object? obj) {
            if (obj is null) {
                _writer.Write("null");
                return;
            }

            if (obj is Type type) {
                _writer.Write($"typeof({type.GetFriendlyName()})");
                return;
            }

            if (Numeric.TryCreate(obj, out var numeric)) {
                _writer.Write(numeric);
                return;
            }

            if (obj is bool b) {
                _writer.Write(b ? "true" : "false");
                return;
            }

            var objType = obj.GetType();
            if (objType.IsPrimitiveData()) {
                _writer.Write('"');
                _writer.Write(obj);
                _writer.Write('"');
                return;
            }

            if (!objType.IsValueType) {
                if (_refInstances.Contains(obj)) {
                    _writer.Write("(circular ref)");
                    return;
                }
                _refInstances.Add(obj);
            }

            _writer.Write('{');
            if (obj is IEnumerable collection) {
                _writer.Indent++;
                var i = 0;
                foreach (var item in collection) {
                    if (i > 0) {
                        _writer.Write(',');
                    }
                    _writer.WriteLine();
                    //_writer.Write('"');
                    _writer.Write(i);
                    //_writer.Write('"');
                    _writer.Write(':');
                    if (_memberSpace) {
                        _writer.Write(' ');
                    }
                    Invoke(item);
                    i++;
                }
                _writer.Indent--;
            }

            if (!objType.IsSystemCollection()) {
                _writer.Indent++;
                var i = 0;
                foreach (DataMember member in objType.GetDataMembers()) {
                    if (i > 0) {
                        _writer.Write(',');
                    }
                    _writer.WriteLine();
                    //_writer.Write('"');
                    _writer.Write(member.Name);
                    //_writer.Write('"');
                    _writer.Write(':');
                    if (_memberSpace) {
                        _writer.Write(' ');
                    }
                    Invoke(member.GetValue(obj));
                    i++;
                }
                _writer.Indent--;
            }
            _writer.WriteLine();
            _writer.Write('}');
        }
    }
}
