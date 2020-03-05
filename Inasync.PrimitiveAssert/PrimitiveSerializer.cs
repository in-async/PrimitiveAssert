using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Inasync {

    public static class PrimitiveSerializer {

        public static string ToPrimitiveString(this object obj) {
            return Serialize(obj);
        }

        public static string Serialize(this object obj) {
            using var stringWriter = new StringWriter();
            using var writer = new IndentedTextWriter(stringWriter, tabString: "    ");

            WritePrimitiveString(obj, new HashSet<object>(), writer);

            writer.Flush();
            return stringWriter.ToString().TrimEnd(',');
        }

        private static void WritePrimitiveString(object obj, HashSet<object> refInstances, IndentedTextWriter writer) {
            if (obj is null) {
                writer.Write("null");
                writer.WriteLine();
                return;
            }

            if (Numeric.TryCreate(obj, out var numeric)) {
                writer.Write(numeric.ToString());
                writer.WriteLine();
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
                    writer.WriteLine("(nested)");
                    return;
                }
                refInstances.Add(obj);
            }
            writer.WriteLine("{");
            writer.Indent++;
            foreach (DataMember member in objType.GetDataMembers()) {
                writer.Write(member.Name + ": ");
                WritePrimitiveString(member.GetValue(obj), refInstances, writer);
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
            writer.Indent--;
            writer.WriteLine("}");
        }
    }
}
