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
            AssertIs(new DeepAssertArgs(actual?.GetType(), actual, expected, path: ""), message);
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
            AssertIs(new DeepAssertArgs(typeof(TTarget), actual, expected, path: ""), message);
        }

        internal static void AssertIs(DeepAssertArgs args, string? message) {
            var (targetType, actual, expected, path) = (args.TargetType, args.Actual, args.Expected, args.Path);

            // null 比較
            if (targetType is null) {
                if (!(actual is null)) { throw new DeepAssertFailedException(args, $"ターゲット型は null ですが、actual は非 null です。", message); }
                if (!(expected is null)) { throw new DeepAssertFailedException(args, $"ターゲット型は null ですが、expected は非 null です。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(args);
                return;
            }
            if (actual is null) {
                if (expected is null) {
                    Debug.WriteLine(message);
                    Debug.WriteLine(args);
                    return;
                }
                else { throw new DeepAssertFailedException(args, $"actual は null だが、expected が非 null。", message); }
            }
            else {
                if (expected is null) { throw new DeepAssertFailedException(args, $"actual は非 null だが、expected が null。", message); }
            }

            // 参照の比較
            if (object.ReferenceEquals(actual, expected)) {
                Debug.WriteLine(message);
                Debug.WriteLine(args);
                return;
            }

            // nullable 解除
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                AssertIs(new DeepAssertArgs(targetType.GetGenericArguments()[0], actual, expected, path + "Nullable+"), message);
                return;
            }

            var (actualType, expectedType) = (actual.GetType(), expected.GetType());

            // 数値型として比較
            if (targetType.IsNumeric()) {
                if (!Numeric.TryCreate(actual, out var actualNumeric)) { throw new DeepAssertFailedException(args, $"ターゲット型 {targetType} は数値型ですが、actual の型 {actualType} は非数値型です。", message); }
                if (!Numeric.TryCreate(expected, out var expectedNumeric)) { throw new DeepAssertFailedException(args, $"ターゲット型 {targetType} は数値型ですが、expected の型 {expectedType} は非数値型です。", message); }
                if (!actualNumeric.Equals(expectedNumeric)) { throw new DeepAssertFailedException(args, $"actual と expected は数値型として等しくありません。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(args);
                return;
            }

            // Primitive Data 型として比較
            if (targetType.IPrimitiveData()) {
                if (!targetType.IsAssignableFrom(actualType)) { throw new DeepAssertFailedException(args, $"ターゲット型 {targetType} は基本データ型ですが、actual の型 {actualType} は非基本データ型です。", message); }
                if (!targetType.IsAssignableFrom(expectedType)) { throw new DeepAssertFailedException(args, $"ターゲット型 {targetType} は基本データ型ですが、expected の型 {expectedType} は非基本データ型です。", message); }
                if (!actual.Equals(expected)) { throw new DeepAssertFailedException(args, $"actual と expected は基本データ型として等しくありません。", message); }

                Debug.WriteLine(message);
                Debug.WriteLine(args);
                return;
            }

            // コレクション型として比較
            if (typeof(IEnumerable).IsAssignableFrom(targetType) && targetType != typeof(string)) {
                if (!(actual is IEnumerable)) { throw new DeepAssertFailedException(args, $"ターゲット型 {targetType} はコレクション型ですが、actual の型 {actualType} は非コレクション型です。", message); }
                if (!(expected is IEnumerable)) { throw new DeepAssertFailedException(args, $"ターゲット型 {targetType} はコレクション型ですが、expected の型 {expectedType} は非コレクション型です。", message); }
                var actualItems = ((IEnumerable)actual).AsCollection();
                var expectedItems = ((IEnumerable)expected).AsCollection();
                if (actualItems.Count != expectedItems.Count) { throw new DeepAssertFailedException(args, $"actual の要素数 {actualItems.Count} と expected の要素数 {expectedItems.Count} が等しくありません。", message); }

                var itemType = targetType.GenericTypeArguments.FirstOrDefault();
                var actualIter = actualItems.GetEnumerator();
                var expectedIter = expectedItems.GetEnumerator();
                for (var i = 0; i < actualItems.Count; i++) {
                    actualIter.MoveNext();
                    expectedIter.MoveNext();

                    var itemTargetType = itemType ?? actualIter.Current?.GetType();
                    AssertIs(new DeepAssertArgs(itemTargetType, actualIter.Current, expectedIter.Current, path + "[" + i + "]"), message);
                }
                return;
            }

            // Composite Data 型として比較
            // NOTE: (オーバーライドされた) Equals による比較には一般的に型の一致も含まれる為、型の比較を行わないここでは使用しない。
            AssertIsByProperties(targetType, actual, expected, path, message);
            AssertIsByFields(targetType, actual, expected, path, message);
        }

        /// <summary>
        /// <paramref name="targetType"/> 型の公開プロパティを基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        private static void AssertIsByProperties(Type targetType, object actual, object expected, string path, string? message) {
            var (actualType, expectedType) = (actual.GetType(), expected.GetType());
            var args = new DeepAssertArgs(targetType, actual, expected, path);

            var props = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props) {
                if (prop.GetIndexParameters().Length > 0) { continue; }

                var actualProp = actualType.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public);
                if (actualProp is null) { throw new DeepAssertFailedException(args, $"actual にプロパティ {prop.Name} が見つかりません。", message); }

                var expectedProp = expectedType.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public);
                if (expectedProp is null) { throw new DeepAssertFailedException(args, $"expected にプロパティ {prop.Name} が見つかりません。", message); }

                var actualPropValue = actualProp.GetValue(actual);
                var expectedPropValue = expectedProp.GetValue(expected);
                AssertIs(new DeepAssertArgs(actualPropValue?.GetType(), actualPropValue, expectedPropValue, path + "." + actualProp.Name), message);
            }
        }

        /// <summary>
        /// <paramref name="targetType"/> 型の公開フィールドを基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        private static void AssertIsByFields(Type targetType, object actual, object expected, string path, string? message) {
            var (actualType, expectedType) = (actual.GetType(), expected.GetType());
            var args = new DeepAssertArgs(targetType, actual, expected, path);

            var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields) {
                var actualField = actualType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public);
                if (actualField is null) { throw new DeepAssertFailedException(args, $"actual にプロパティ {field.Name} が見つかりません。", message); }

                var expectedField = expectedType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public);
                if (expectedField is null) { throw new DeepAssertFailedException(args, $"expected にプロパティ {field.Name} が見つかりません。", message); }

                var actualPropValue = actualField.GetValue(actual);
                var expectedPropValue = expectedField.GetValue(expected);
                AssertIs(new DeepAssertArgs(actualPropValue?.GetType(), actualPropValue, expectedPropValue, path + "." + actualField.Name), message);
            }
        }
    }
}
