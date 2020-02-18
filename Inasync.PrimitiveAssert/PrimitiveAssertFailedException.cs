using System;

namespace Inasync {

    /// <summary>
    /// <see cref="PrimitiveAssert.AssertIs{TTarget}(object, object, string)"/> に失敗した時に送出される例外を表します。
    /// </summary>
    public class PrimitiveAssertFailedException : Exception {

        /// <summary>
        /// 指定したエラー メッセージを使用して、<see cref="PrimitiveAssertFailedException"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="message">エラーを説明するメッセージ。</param>
        public PrimitiveAssertFailedException(string? message) : base(message) { }

        internal PrimitiveAssertFailedException(AssertNode node, string reason, string? message) : base($@"{message}: {reason}
{node}") { }
    }
}
