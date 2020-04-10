using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Commons;

namespace Inasync {

    public static class PrimitiveSerializer {

        public static string ToPrimitiveString(this object? obj, bool pretty = false) {
            using var writer = new StringWriter();
            Serialize(obj, writer, pretty);
            writer.Flush();
            return writer.ToString();
        }

        public static void Serialize(object? obj, TextWriter writer, bool pretty = false) {
            using var indentedWriter = new IndentedTextWriter(writer, tabString: pretty ? "    " : "");
            if (!pretty) {
                indentedWriter.NewLine = " ";
            }

            WritePrimitiveString(obj, new HashSet<object>(), indentedWriter);
        }

        private static void WritePrimitiveString(object? obj, HashSet<object> refInstances, IndentedTextWriter writer) {
            if (obj is null) {
                writer.WriteLine("null");
                return;
            }

            if (obj is Type type) {
                writer.WriteLine(type?.GetFriendlyName() ?? "(null)");
                return;
            }

            if (Numeric.TryCreate(obj, out var numeric)) {
                writer.WriteLine(numeric);
                return;
            }

            if (obj is bool b) {
                writer.Write(b ? "true" : "false");
                writer.WriteLine();
                return;
            }

            var objType = obj.GetType();
            if (objType.IsPrimitiveData()) {
                writer.Write('"');
                writer.Write(obj.ToString());
                writer.Write('"');
                writer.WriteLine();
                return;
            }

            if (!objType.IsValueType) {
                if (refInstances.Contains(obj)) {
                    writer.WriteLine("(circular ref)");
                    return;
                }
                refInstances.Add(obj);
            }

            if (obj is IEnumerable collection) {
                writer.WriteLine("[");
                writer.Indent++;
                foreach (var item in collection) {
                    WritePrimitiveString(item, refInstances, writer);
                }
                writer.Indent--;
                writer.WriteLine("]");
            }

            if (!objType.IsSystemCollection()) {
                writer.WriteLine("{");
                writer.Indent++;
                foreach (DataMember member in objType.GetDataMembers()) {
                    writer.Write(member.Name + ": ");
                    WritePrimitiveString(member.GetValue(obj), refInstances, writer);
                }
                writer.Indent--;
                writer.WriteLine("}");
            }
        }
    }
}
