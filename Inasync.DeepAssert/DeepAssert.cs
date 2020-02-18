using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Inasync {

    /// <summary>
    /// ターゲットを基本データ型に分解して比較します。
    /// </summary>
    public static class DeepAssert {

        /// <summary>
        /// <paramref name="actual"/> のランタイム型を比較の基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        /// <param name="actual">検証対象の実値。</param>
        /// <param name="expected">比較対象となる期待値。</param>
        /// <param name="message">検証に失敗した際に、例外に含まれるメッセージ。</param>
        /// <exception cref="DeepAssertFailedException"><paramref name="actual"/> と <paramref name="expected"/> が等価ではありません。</exception>
        public static void AssertIs(this object? actual, object? expected, string? message = null) {
            AssertIs(new AssertNode(name: "", actual?.GetType(), actual, expected, parent: null), message);
        }

        /// <summary>
        /// <typeparamref name="TTarget"/> 型を比較の基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        /// <typeparam name="TTarget">比較の基準となる型。</typeparam>
        /// <param name="actual">検証対象の実値。</param>
        /// <param name="expected">比較対象となる期待値。</param>
        /// <param name="message">検証に失敗した際に、例外に含まれるメッセージ。</param>
        /// <exception cref="DeepAssertFailedException"><paramref name="actual"/> と <paramref name="expected"/> が等価ではありません。</exception>
        public static void AssertIs<TTarget>(this object? actual, object? expected, string? message = null) {
            AssertIs(new AssertNode(name: "", targetType: typeof(TTarget), actual, expected, parent: null), message);
        }

        internal static void AssertIs(AssertNode node, string? message) {
            var (targetType, actual, expected) = (node.TargetType, node.Actual, node.Expected);

            // null 比較
            if (targetType is null) {
                if (!(actual is null)) { throw new DeepAssertFailedException(node, $"ターゲット型は null ですが、actual は非 null です。", message); }
                if (!(expected is null)) { throw new DeepAssertFailedException(node, $"ターゲット型は null ですが、expected は非 null です。", message); }

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
                else { throw new DeepAssertFailedException(node, $"actual は null だが、expected が非 null。", message); }
            }
            else {
                if (expected is null) { throw new DeepAssertFailedException(node, $"actual は非 null だが、expected が null。", message); }
            }

            // 参照の比較
            if (object.ReferenceEquals(actual, expected)) {
                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return;
            }

            // nullable 解除
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                AssertIs(new AssertNode("Nullable", targetType.GetGenericArguments()[0], actual, expected, node), message);
                return;
            }

            var (actualType, expectedType) = (actual.GetType(), expected.GetType());

            // 数値型として比較
            if (targetType.IsNumeric()) {
                if (!Numeric.TryCreate(actual, out var actualNumeric)) { throw new DeepAssertFailedException(node, $"ターゲット型 {targetType} は数値型ですが、actual の型 {actualType} は非数値型です。", message); }
                if (!Numeric.TryCreate(expected, out var expectedNumeric)) { throw new DeepAssertFailedException(node, $"ターゲット型 {targetType} は数値型ですが、expected の型 {expectedType} は非数値型です。", message); }
                if (!actualNumeric.Equals(expectedNumeric)) { throw new DeepAssertFailedException(node, $"actual と expected は数値型として等しくありません。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return;
            }

            // Primitive Data 型として比較
            if (targetType.IsPrimitiveData()) {
                if (!targetType.IsAssignableFrom(actualType)) { throw new DeepAssertFailedException(node, $"ターゲット型 {targetType} は基本データ型ですが、actual の型 {actualType} は非基本データ型です。", message); }
                if (!targetType.IsAssignableFrom(expectedType)) { throw new DeepAssertFailedException(node, $"ターゲット型 {targetType} は基本データ型ですが、expected の型 {expectedType} は非基本データ型です。", message); }
                if (!actual.Equals(expected)) { throw new DeepAssertFailedException(node, $"actual と expected は基本データ型として等しくありません。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(node);
                return;
            }

            // コレクション型として比較
            if (typeof(IEnumerable).IsAssignableFrom(targetType) && targetType != typeof(string)) {
                if (!(actual is IEnumerable)) { throw new DeepAssertFailedException(node, $"ターゲット型 {targetType} はコレクション型ですが、actual の型 {actualType} は非コレクション型です。", message); }
                if (!(expected is IEnumerable)) { throw new DeepAssertFailedException(node, $"ターゲット型 {targetType} はコレクション型ですが、expected の型 {expectedType} は非コレクション型です。", message); }
                var actualItems = ((IEnumerable)actual).AsCollection();
                var expectedItems = ((IEnumerable)expected).AsCollection();
                if (actualItems.Count != expectedItems.Count) { throw new DeepAssertFailedException(node, $"actual の要素数 {actualItems.Count} と expected の要素数 {expectedItems.Count} が等しくありません。", message); }

                var itemType = targetType.GenericTypeArguments.FirstOrDefault();
                var actualIter = actualItems.GetEnumerator();
                var expectedIter = expectedItems.GetEnumerator();
                for (var i = 0; i < actualItems.Count; i++) {
                    actualIter.MoveNext();
                    expectedIter.MoveNext();

                    var itemTargetType = itemType ?? actualIter.Current?.GetType();
                    AssertIs(new AssertNode(i.ToString(), itemTargetType, actualIter.Current, expectedIter.Current, node), message);
                }
                return;
            }

            // Composite Data 型として比較
            // NOTE: (オーバーライドされた) Equals による比較には一般的に型の一致も含まれる為、型の比較を行わないここでは使用しない。
            AssertIsByProperties(targetType, actual, expected, node, message);
            AssertIsByFields(targetType, actual, expected, node, message);
        }

        /// <summary>
        /// <paramref name="targetType"/> 型の公開プロパティを基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        private static void AssertIsByProperties(Type targetType, object actual, object expected, AssertNode node, string? message) {
            var (actualType, expectedType) = (actual.GetType(), expected.GetType());

            var props = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props) {
                if (prop.GetIndexParameters().Length > 0) { continue; }

                var actualProp = actualType.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public);
                if (actualProp is null) { throw new DeepAssertFailedException(node, $"actual にプロパティ {prop.Name} が見つかりません。", message); }

                var expectedProp = expectedType.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public);
                if (expectedProp is null) { throw new DeepAssertFailedException(node, $"expected にプロパティ {prop.Name} が見つかりません。", message); }

                var actualPropValue = actualProp.GetValue(actual);
                var expectedPropValue = expectedProp.GetValue(expected);
                AssertIs(new AssertNode(actualProp.Name, prop.PropertyType, actualPropValue, expectedPropValue, node), message);
            }
        }

        /// <summary>
        /// <paramref name="targetType"/> 型の公開フィールドを基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        private static void AssertIsByFields(Type targetType, object actual, object expected, AssertNode node, string? message) {
            var (actualType, expectedType) = (actual.GetType(), expected.GetType());

            var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields) {
                var actualField = actualType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public);
                if (actualField is null) { throw new DeepAssertFailedException(node, $"actual にフィールド {field.Name} が見つかりません。", message); }

                var expectedField = expectedType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public);
                if (expectedField is null) { throw new DeepAssertFailedException(node, $"expected にフィールド {field.Name} が見つかりません。", message); }

                var actualFieldValue = actualField.GetValue(actual);
                var expectedFieldValue = expectedField.GetValue(expected);
                AssertIs(new AssertNode(actualField.Name, field.FieldType, actualFieldValue, expectedFieldValue, node), message);
            }
        }
    }
}
