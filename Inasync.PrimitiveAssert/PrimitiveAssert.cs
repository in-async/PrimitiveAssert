using System;

namespace Inasync {

    /// <summary>
    /// ターゲットを基本データ型に分解して比較します。
    /// </summary>
    public static class PrimitiveAssert {
        private static readonly Action<string> _consoleLogger = x => Console.WriteLine(x);

        /// <summary>
        /// Assert のログをコンソールに出力するかどうか。
        /// <para>既定値は <see langword="false"/>。</para>
        /// </summary>
        public static bool ConsoleLogging { get; set; } = false;

        /// <summary>
        /// <typeparamref name="TTarget"/> 型を比較の基準とし、
        /// <paramref name="actual"/> と <paramref name="expected"/> が等価かどうかを検証します。
        /// </summary>
        /// <param name="actual">検証対象の実値。</param>
        /// <param name="expected">比較対象となる期待値。</param>
        /// <param name="message">検証に失敗した際に、例外に含まれるメッセージ。</param>
        /// <exception cref="PrimitiveAssertFailedException"><paramref name="actual"/> と <paramref name="expected"/> が等価ではありません。</exception>
        /// <exception cref="ArgumentException">ターゲット型に同じ名前のデータメンバーが 2 つ以上存在します。</exception>
        public static void AssertIs<TTarget>(this TTarget actual, object? expected, string? message = null) {
            actual.AssertIs(typeof(TTarget), expected, message);
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
        /// <exception cref="ArgumentException">ターゲット型に同じ名前のデータメンバーが 2 つ以上存在します。</exception>
        /// <remarks><paramref name="actual"/> と <paramref name="expected"/> に対して対称式となります。</remarks>
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
        /// <exception cref="ArgumentException">ターゲット型に同じ名前のデータメンバーが 2 つ以上存在します。</exception>
        /// <remarks><paramref name="actual"/> と <paramref name="expected"/> に対して対称式となります。</remarks>
        public static void AssertIs(this object? actual, Type? targetType, object? expected, string? message = null) {
            var assert = new AssertIsImpl(message, ConsoleLogging ? _consoleLogger : null);
            assert.AssertIs(new AssertNode(memberName: "", targetType: targetType, actual, expected, parent: null));
        }
    }
}
