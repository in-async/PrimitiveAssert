using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Inasync {

    /// <summary>
    /// ターゲットを基本データ型に分解して比較します。
    /// </summary>
    public static class PrimitiveAssert {

        /// <summary>
        /// <paramref name="actual"/> のランタイム型を比較の基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
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

        public static void AssertIs(this object? actual, Type? targetType, object? expected, string? message = null) {
            RootAssert.AssertIs(new AssertNode(memberName: "", targetType: targetType, actual, expected, parent: null), message);
        }
    }

    internal static class RootAssert {

        public static void AssertIs(AssertNode node, string? message) {
            var (targetType, actual, expected) = (node.TargetType, node.Actual, node.Expected);

            // null 比較
            if (targetType is null) {
                if (!(actual is null)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型は null ですが、actual は非 null です。", message); }
                if (!(expected is null)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型は null ですが、expected は非 null です。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return;
            }
            if (actual is null) {
                if (expected is null) {
                    Debug.WriteLine(message);
                    Debug.WriteLine(node);
                    return;
                }
                else { throw new PrimitiveAssertFailedException(node, $"actual は null だが、expected が非 null。", message); }
            }
            else {
                if (expected is null) { throw new PrimitiveAssertFailedException(node, $"actual は非 null だが、expected が null。", message); }
            }

            // 参照の比較
            if (object.ReferenceEquals(actual, expected)) {
                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return;
            }

            if (PrimitiveNodeAssert.TryAssertIs(targetType, actual, expected, node, message)) { return; }
            if (CollectionNodeAssert.TryAssertIs(targetType, actual, expected, node, message)) { return; }
            CompositeNodeAssert.AssertIs(targetType, actual, expected, node, message);
        }
    }

    internal static class PrimitiveNodeAssert {

        public static bool TryAssertIs(Type targetType, object actual, object expected, AssertNode node, string? message) {
            var (actualType, expectedType) = (actual.GetType(), expected.GetType());

            // 数値型として比較
            if (targetType.IsNumeric()) {
                if (!Numeric.TryCreate(actual, out var actualNumeric)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} は数値型ですが、actual の型 {actualType} は非数値型です。", message); }
                if (!Numeric.TryCreate(expected, out var expectedNumeric)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} は数値型ですが、expected の型 {expectedType} は非数値型です。", message); }
                if (!actualNumeric.Equals(expectedNumeric)) { throw new PrimitiveAssertFailedException(node, $"actual と expected は数値型として等しくありません。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return true;
            }

            // Primitive Data 型として比較
            if (targetType.IsPrimitiveData()) {
                if (!targetType.IsAssignableFrom(actualType)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} は基本データ型ですが、actual の型 {actualType} は非基本データ型です。", message); }
                if (!targetType.IsAssignableFrom(expectedType)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} は基本データ型ですが、expected の型 {expectedType} は非基本データ型です。", message); }
                if (!actual.Equals(expected)) { throw new PrimitiveAssertFailedException(node, $"actual と expected は基本データ型として等しくありません。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return true;
            }

            return false;
        }
    }

    internal static class CollectionNodeAssert {

        public static bool TryAssertIs(Type targetType, object actual, object expected, AssertNode node, string? message) {
            if (!typeof(IEnumerable).IsAssignableFrom(targetType)) { return false; }
            if (targetType == typeof(string)) { return false; }

            var (actualType, expectedType) = (actual.GetType(), expected.GetType());

            if (!(actual is IEnumerable)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} はコレクション型ですが、actual の型 {actualType} は非コレクション型です。", message); }
            if (!(expected is IEnumerable)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} はコレクション型ですが、expected の型 {expectedType} は非コレクション型です。", message); }
            var actualItems = ((IEnumerable)actual).AsCollection();
            var expectedItems = ((IEnumerable)expected).AsCollection();
            if (actualItems.Count != expectedItems.Count) { throw new PrimitiveAssertFailedException(node, $"actual の要素数 {actualItems.Count} と expected の要素数 {expectedItems.Count} が等しくありません。", message); }

            var itemType = targetType.GenericTypeArguments.FirstOrDefault();
            var actualIter = actualItems.GetEnumerator();
            var expectedIter = expectedItems.GetEnumerator();
            for (var i = 0; i < actualItems.Count; i++) {
                actualIter.MoveNext();
                expectedIter.MoveNext();

                var itemTargetType = itemType ?? actualIter.Current?.GetType();
                RootAssert.AssertIs(new AssertNode(i.ToString(), itemTargetType, actualIter.Current, expectedIter.Current, node), message);
            }

            CompositeNodeAssert.AssertIs(targetType, actual, expected, node, message);

            return true;
        }
    }

    internal static class CompositeNodeAssert {

        public static void AssertIs(Type targetType, object actual, object expected, AssertNode node, string? message) {
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            var (actualType, expectedType) = (actual.GetType(), expected.GetType());
            if (targetType.IsAssignableFrom(actualType)) {
                actualType = targetType;
            }
            if (targetType.IsAssignableFrom(expectedType)) {
                expectedType = targetType;
            }

            var props = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props) {
                if (prop.GetIndexParameters().Length > 0) { continue; }

                var actualProp = actualType.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public);
                if (actualProp is null) { throw new PrimitiveAssertFailedException(node, $"actual にプロパティ {prop.Name} が見つかりません。", message); }

                var expectedProp = expectedType.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public);
                if (expectedProp is null) { throw new PrimitiveAssertFailedException(node, $"expected にプロパティ {prop.Name} が見つかりません。", message); }

                var actualPropValue = actualProp.GetValue(actual);
                var expectedPropValue = expectedProp.GetValue(expected);
                RootAssert.AssertIs(new AssertNode(actualProp.Name, prop.PropertyType, actualPropValue, expectedPropValue, node), message);
            }

            var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields) {
                var actualField = actualType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public);
                if (actualField is null) { throw new PrimitiveAssertFailedException(node, $"actual にフィールド {field.Name} が見つかりません。", message); }

                var expectedField = expectedType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public);
                if (expectedField is null) { throw new PrimitiveAssertFailedException(node, $"expected にフィールド {field.Name} が見つかりません。", message); }

                var actualFieldValue = actualField.GetValue(actual);
                var expectedFieldValue = expectedField.GetValue(expected);
                RootAssert.AssertIs(new AssertNode(actualField.Name, field.FieldType, actualFieldValue, expectedFieldValue, node), message);
            }
        }
    }
}
