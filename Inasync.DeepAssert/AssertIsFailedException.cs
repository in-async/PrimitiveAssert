using System;

namespace Inasync {

    /// <summary>
    /// <see cref="DeepAssert.AssertIs{TTarget}(object, object, string)"/> に失敗した時に送出される例外を表します。
    /// </summary>
    public class AssertIsFailedException : Exception {

        /// <summary>
        /// 指定したエラー メッセージを使用して、<see cref="AssertIsFailedException"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="message">エラーを説明するメッセージ。</param>
        public AssertIsFailedException(string? message) : base(message) { }

        internal AssertIsFailedException(AssertIsArgs args, string reason, string? message) : base($@"{message}: {reason}
{args}") { }
    }
}
