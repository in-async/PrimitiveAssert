using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Commons;

namespace Inasync {

    internal static class TypeExtensions {

        /// <summary>
        /// <see cref="PrimitiveAssert"/> におけるプリミティブ データ型かどうか。
        /// </summary>
        public static bool IsPrimitiveData(this Type type) {
            return type == typeof(bool)
                || type == typeof(char)
                || type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
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
                || Nullable.GetUnderlyingType(type) is Type underingType && IsPrimitiveData(underingType)
                ;
        }

        /// <summary>
        /// 汎用コレクションかどうか。
        /// <para>
        /// ここでの汎用コレクションとは配列、<see cref="Array"/>、
        /// <see cref="System.Collections"/> または <see cref="System.Collections.Generic"/> 名前空間に属する <see cref="IEnumerable"/> 実装のいずれかを指します。
        /// </para>
        /// </summary>
        public static bool IsSystemCollection(this Type type) {
            if (!typeof(IEnumerable).IsAssignableFrom(type)) { return false; }
            if (typeof(Array).IsAssignableFrom(type)) { return true; }

            return type.Namespace switch
            {
                "System.Collections" => true,
                "System.Collections.Generic" => true,
                _ => false,
            };
        }

        /// <summary>
        /// インスタンスかつパブリックな <see cref="DataMember"/> の一覧を返します。
        /// </summary>
        /// <remarks>
        /// 多重継承したインターフェース等が指定された場合に、同じ名前のデータメンバーが列挙される可能性があります。
        /// これを一意に識別する場合は、宣言された型 (DeclaringType) も併せて識別子として下さい。
        /// </remarks>
        public static IEnumerable<DataMember> GetDataMembers(this Type type) {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            var props = (
                from prop in type.GetPropertiesEx(bindingFlags)
                where prop.GetIndexParameters().Length == 0
                where prop.GetGetMethod() != null
                select new DataMember(prop)
            );

            var fields = (
                from field in type.GetFields(bindingFlags)
                select new DataMember(field)
            );

            return props.Concat(fields);
        }

        /// <summary>
        /// <paramref name="duckType"/> のインスタンスかつパブリックな <see cref="DataMember"/> が全て実装されているかどうか。
        /// </summary>
        public static bool IsDuckImplemented(this Type type, Type duckType) {
            if (duckType.IsAssignableFrom(type)) { return true; }

            var targetMembers = type.GetDataMembers().GroupBy(x => x.Name).ToDictionary(g => g.Key, g => g.Count());
            var duckMembers = duckType.GetDataMembers().GroupBy(x => x.Name).ToDictionary(g => g.Key, g => g.Count());
            var duckMemberSet = new HashSet<KeyValuePair<string, int>>(duckMembers);
            return duckMemberSet.IsSubsetOf(targetMembers);
        }
    }
}
