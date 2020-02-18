using System;

namespace Inasync {

    /// <summary>
    /// <see cref="DeepAssert.AssertIs{TTarget}(object, object, string)"/> に失敗した時に送出される例外を表します。
    /// </summary>
    public class DeepAssertFailedException : Exception {

        /// <summary>
        /// 指定したエラー メッセージを使用して、<see cref="DeepAssertFailedException"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="message">エラーを説明するメッセージ。</param>
        public DeepAssertFailedException(string? message) : base(message) { }

        internal DeepAssertFailedException(AssertNode node, string reason, string? message) : base($@"{message}: {reason}
{node}") { }
    }
}
