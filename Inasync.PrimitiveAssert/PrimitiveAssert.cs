using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Inasync {

    /// <summary>
    /// ターゲットを基本データ型に分解して比較します。
    /// </summary>
    public static class PrimitiveAssert {
        private static readonly RootAssert _assert = new RootAssert();

        /// <summary>
        /// <paramref name="actual"/> のランタイム型を比較の基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        /// <remarks><paramref name="actual"/> と <paramref name="expected"/> に対して対称式となります。</remarks>
        /// <param name="actual">検証対象の実値。</param>
        /// <param name="expected">比較対象となる期待値。</param>
        /// <param name="message">検証に失敗した際に、例外に含まれるメッセージ。</param>
        /// <exception cref="PrimitiveAssertFailedException"><paramref name="actual"/> と <paramref name="expected"/> が等価ではありません。</exception>
        public static void AssertIs(this object? actual, object? expected, string? message = null) {
            actual.AssertIs(actual?.GetType(), expected, message);
        }

        /// <summary>
        /// <typeparamref name="TTarget"/> 型を比較の基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        /// <typeparam name="TTarget">比較の基準となる型。</typeparam>
        /// <param name="actual">検証対象の実値。</param>
        /// <param name="expected">比較対象となる期待値。</param>
        /// <param name="message">検証に失敗した際に、例外に含まれるメッセージ。</param>
        /// <exception cref="PrimitiveAssertFailedException"><paramref name="actual"/> と <paramref name="expected"/> が等価ではありません。</exception>
        public static void AssertIs<TTarget>(this object? actual, object? expected, string? message = null) {
            actual.AssertIs(typeof(TTarget), expected, message);
        }

        /// <summary>
        /// <paramref name="targetType"/> 型を比較の基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        /// <param name="actual">検証対象の実値。</param>
        /// <param name="targetType">比較の基準となる型。</param>
        /// <param name="expected">比較対象となる期待値。</param>
        /// <param name="message">検証に失敗した際に、例外に含まれるメッセージ。</param>
        /// <exception cref="PrimitiveAssertFailedException"><paramref name="actual"/> と <paramref name="expected"/> が等価ではありません。</exception>
        public static void AssertIs(this object? actual, Type? targetType, object? expected, string? message = null) {
            _assert.AssertIs(new AssertNode(memberName: "", targetType: targetType, actual, expected, parent: null), message);
        }
    }

    internal class RootAssert {

        public void AssertIs(AssertNode node, string? message) {
            var (targetType, actual, expected) = (node.TargetType, node.Actual, node.Expected);

            // null 比較
            if (targetType is null) {
                if (!(actual is null)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は null ですが、actual は非 null です。", message); }
                if (!(expected is null)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は null ですが、expected は非 null です。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return;
            }
            if (targetType.IsValueType && !targetType.IsNullable()) {
                if (actual is null) { throw new PrimitiveAssertFailedException(node, "ターゲット型は null 非許容型ですが、actual は null です。", message); }
                if (expected is null) { throw new PrimitiveAssertFailedException(node, "ターゲット型は null 非許容型ですが、expected は null です。", message); }
            }
            if (actual is null) {
                if (expected is null) {
                    Debug.WriteLine(message);
                    Debug.WriteLine(node);
                    return;
                }
                else { throw new PrimitiveAssertFailedException(node, "actual は null ですが、expected が非 null です。", message); }
            }
            else {
                if (expected is null) { throw new PrimitiveAssertFailedException(node, "actual は非 null ですが、expected が null です。", message); }
            }

            if (TryNumericAssertIs(targetType, actual, expected, node, message)) { return; }
            if (TryPrimitiveAssertIs(targetType, actual, expected, node, message)) { return; }
            if (TryCompositeAssertIs(targetType, actual, expected, node, message)) { return; }
            if (TryCollectionAssertIs(targetType, actual, expected, node, message)) { return; }
        }

        protected virtual bool TryNumericAssertIs(Type targetType, object actual, object expected, AssertNode node, string? message) {
            if (!targetType.IsNumeric()) { return false; }

            if (!Numeric.TryCreate(actual, out var actualNumeric)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は数値型ですが、actual は非数値型です。", message); }
            if (!Numeric.TryCreate(expected, out var expectedNumeric)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は数値型ですが、expected は非数値型です。", message); }
            if (!actualNumeric.Equals(expectedNumeric)) { throw new PrimitiveAssertFailedException(node, "actual と expected は数値型として等しくありません。", message); }

            Debug.WriteLine(message);
            Debug.WriteLine(node);
            return true;
        }

        protected virtual bool TryPrimitiveAssertIs(Type targetType, object actual, object expected, AssertNode node, string? message) {
            if (!targetType.IsPrimitiveData()) { return false; }

            if (!targetType.IsInstanceOfType(actual)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は基本データ型ですが、actual はターゲット型に違反しています。", message); }
            if (!targetType.IsInstanceOfType(expected)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は基本データ型ですが、expected はターゲット型に違反しています。", message); }
            if (!actual.Equals(expected)) { throw new PrimitiveAssertFailedException(node, $"actual と expected は基本データ型として等しくありません。", message); }

            Debug.WriteLine(message);
            Debug.WriteLine(node);
            return true;
        }

        protected virtual bool TryCollectionAssertIs(Type targetType, object actual, object expected, AssertNode node, string? message) {
            if (!typeof(IEnumerable).IsAssignableFrom(targetType)) { return false; }
            if (targetType == typeof(string)) { return false; }

            var (actualType, expectedType) = (actual.GetType(), expected.GetType());

            if (!(actual is IEnumerable)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} はコレクション型ですが、actual の型 {actualType} は非コレクション型です。", message); }
            if (!(expected is IEnumerable)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} はコレクション型ですが、expected の型 {expectedType} は非コレクション型です。", message); }
            var actualItems = ((IEnumerable)actual).AsCollection();
            var expectedItems = ((IEnumerable)expected).AsCollection();
            if (actualItems.Count != expectedItems.Count) { throw new PrimitiveAssertFailedException(node, $"actual の要素数 {actualItems.Count} と expected の要素数 {expectedItems.Count} が等しくありません。", message); }

            var itemType = targetType.IsArray ? targetType.GetElementType() : targetType.GenericTypeArguments.FirstOrDefault();
            var actualIter = actualItems.GetEnumerator();
            var expectedIter = expectedItems.GetEnumerator();
            for (var i = 0; i < actualItems.Count; i++) {
                actualIter.MoveNext();
                expectedIter.MoveNext();

                var itemTargetType = itemType ?? actualIter.Current?.GetType();
                AssertIs(new AssertNode(i.ToString(), itemTargetType, actualIter.Current, expectedIter.Current, node), message);
            }

            return false;
        }

        protected virtual bool TryCompositeAssertIs(Type targetType, object actual, object expected, AssertNode node, string? message) {
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            var (actualType, expectedType) = (actual.GetType(), expected.GetType());
            if (targetType.IsAssignableFrom(actualType)) {
                actualType = targetType;
            }
            if (targetType.IsAssignableFrom(expectedType)) {
                expectedType = targetType;
            }

            // 参照の比較
            if (object.ReferenceEquals(actual, expected)) {
                if (!actualType.IsDuckImplemented(targetType)) { throw new PrimitiveAssertFailedException(node, "actual と expected は参照等価ですが、ターゲット型に違反しています。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return true;
            }

            // 各データ メンバーの比較
            foreach (var member in targetType.GetDataMembers()) {
                var actualMember = actualType.GetDataMember(member.Name);
                if (actualMember is null) { throw new PrimitiveAssertFailedException(node, $"actual にデータ メンバー {member.Name} が見つかりません。", message); }

                var expectedMember = expectedType.GetDataMember(member.Name);
                if (expectedMember is null) { throw new PrimitiveAssertFailedException(node, $"expected にデータ メンバー {member.Name} が見つかりません。", message); }

                var actualMemberValue = actualMember.Value.GetValue(actual);
                var expectedMemberValue = expectedMember.Value.GetValue(expected);
                AssertIs(new AssertNode(member.Name, member.DataType, actualMemberValue, expectedMemberValue, node), message);
            }

            return false;
        }
    }

    internal static class DuckExtensions {

        public static bool IsDuckImplemented(this Type type, Type duckType) {
            if (duckType.IsAssignableFrom(type)) { return true; }

            var objMembers = type.GetDataMembers().Select(x => x.Name);
            var duckMemberSet = new HashSet<string>(duckType.GetDataMembers().Select(x => x.Name));
            return duckMemberSet.IsSubsetOf(objMembers);
        }

        public static IEnumerable<DataMember> GetDataMembers(this Type type) {
            var props = (
                from prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where prop.GetIndexParameters().Length == 0
                where prop.GetGetMethod() != null
                select new DataMember(prop)
            );

            var fields = (
                from field in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                select new DataMember(field)
            );

            return props.Concat(fields);
        }

        public static DataMember? GetDataMember(this Type type, string name) {
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (prop != null) {
                return new DataMember(prop);
            }

            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null) {
                return new DataMember(field);
            }

            return null;
        }
    }

    internal readonly struct DataMember {
        private readonly PropertyInfo? _prop;
        private readonly FieldInfo? _field;

        public readonly string Name;
        public readonly Type DataType;

        public DataMember(PropertyInfo prop) {
            _prop = prop;
            _field = null;
            Name = prop.Name;
            DataType = prop.PropertyType;
        }

        public DataMember(FieldInfo field) {
            _prop = null;
            _field = field;
            Name = field.Name;
            DataType = field.FieldType;
        }

        public object GetValue(object obj) {
            if (_prop != null) { return _prop.GetValue(obj); }
            return _field!.GetValue(obj);
        }
    }
}
