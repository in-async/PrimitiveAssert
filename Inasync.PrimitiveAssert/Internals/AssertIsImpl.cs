using System;
using System.Collections;
using Commons;

namespace Inasync {

    internal readonly struct AssertIsImpl {
        private readonly string? _message;
        private readonly Action<string>? _logger;

        public AssertIsImpl(string? message, Action<string>? logger) {
            _message = message;
            _logger = logger;
        }

        public void AssertIs(AssertNode node) {
            var targetType = node.TargetType;
            var actual = node.Actual;
            var expected = node.Expected;

            // null 比較
            if (targetType is null) {
                if (!(actual is null)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は null ですが、actual は非 null です。", _message); }
                if (!(expected is null)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は null ですが、expected は非 null です。", _message); }

                WriteLog(node, "actual と expected はどちらも null です。");
                return;
            }
            if (targetType.IsValueType && !targetType.IsNullable()) {
                if (actual is null) { throw new PrimitiveAssertFailedException(node, "ターゲット型は null 非許容型ですが、actual は null です。", _message); }
                if (expected is null) { throw new PrimitiveAssertFailedException(node, "ターゲット型は null 非許容型ですが、expected は null です。", _message); }
            }
            if (actual is null) {
                if (expected is null) {
                    WriteLog(node, "actual と expected はどちらも null です。");
                    return;
                }
                else { throw new PrimitiveAssertFailedException(node, "actual は null ですが、expected が非 null です。", _message); }
            }
            else {
                if (expected is null) { throw new PrimitiveAssertFailedException(node, "actual は非 null ですが、expected が null です。", _message); }
            }

            if (TryCircularReferenceAssertIs(targetType, actual, expected, node)) { return; }
            if (TryNumericAssertIs(targetType, actual, expected, node)) { return; }
            if (TryPrimitiveAssertIs(targetType, actual, expected, node)) { return; }
            if (TryCollectionAssertIs(targetType, actual, expected, node)) { return; }
            if (TryCompositeAssertIs(targetType, actual, expected, node)) { return; }
        }

        private void WriteLog(AssertNode node, string additionalMessage) {
            if (_logger == null) { return; }

            if (_message != null) {
                _logger(_message);
            }
            _logger(additionalMessage);
            _logger(node.ToString());
        }

        private bool TryCircularReferenceAssertIs(Type targetType, object actual, object expected, AssertNode node) {
            var actualType = actual.GetType();
            var expectedType = expected.GetType();
            if (actualType.IsValueType || expectedType.IsValueType) { return false; }

            for (var parent = node.Parent; parent != null; parent = parent.Parent) {
                if (ReferenceEquals(parent.Actual, actual)) {
                    if (ReferenceEquals(parent.Expected, expected)) {
                        if (!actualType.IsDuckImplemented(targetType)) { throw new PrimitiveAssertFailedException(node, "actual と expected は同じパスに対する循環参照ですが、ターゲット型に違反しています。", _message); }

                        WriteLog(node, $"actual と expected は同じパスに対する循環参照です。{Environment.NewLine}循環先のパス: {parent.Path}");
                        return true;
                    }
                    else { throw new PrimitiveAssertFailedException(node, "actual は循環参照ですが、expected が循環参照ではありません。", _message); }
                }
                else {
                    if (ReferenceEquals(parent.Expected, expected)) { throw new PrimitiveAssertFailedException(node, "actual は循環参照ではありませんが、expected が循環参照です。", _message); }
                }
            }
            return false;
        }

        private bool TryNumericAssertIs(Type targetType, object actual, object expected, AssertNode node) {
            if (!Numeric.IsNumeric(targetType)) { return false; }

            if (!Numeric.TryCreate(actual, out var actualNumeric)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は数値型ですが、actual は非数値型です。", _message); }
            if (!Numeric.TryCreate(expected, out var expectedNumeric)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は数値型ですが、expected は非数値型です。", _message); }
            if (!actualNumeric.Equals(expectedNumeric)) { throw new PrimitiveAssertFailedException(node, "actual と expected は数値型として等しくありません。", _message); }

            WriteLog(node, "actual と expected は数値型として等しいです。");
            return true;
        }

        private bool TryPrimitiveAssertIs(Type targetType, object actual, object expected, AssertNode node) {
            if (!targetType.IsPrimitiveData()) { return false; }

            if (!targetType.IsInstanceOfType(actual)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は基本データ型ですが、actual はターゲット型に違反しています。", _message); }
            if (!targetType.IsInstanceOfType(expected)) { throw new PrimitiveAssertFailedException(node, "ターゲット型は基本データ型ですが、expected はターゲット型に違反しています。", _message); }
            if (!actual.Equals(expected)) { throw new PrimitiveAssertFailedException(node, $"actual と expected は基本データ型として等しくありません。", _message); }

            WriteLog(node, $"actual と expected は {targetType.Name} 型として等しいです。");
            return true;
        }

        private bool TryCollectionAssertIs(Type targetType, object actual, object expected, AssertNode node) {
            if (!typeof(IEnumerable).IsAssignableFrom(targetType)) { return false; }
            if (targetType == typeof(string)) { return false; }

            var actualType = actual.GetType();
            var expectedType = expected.GetType();

            if (!(actual is IEnumerable)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} はコレクション型ですが、actual の型 {actualType} は非コレクション型です。", _message); }
            if (!(expected is IEnumerable)) { throw new PrimitiveAssertFailedException(node, $"ターゲット型 {targetType} はコレクション型ですが、expected の型 {expectedType} は非コレクション型です。", _message); }
            var actualItems = ((IEnumerable)actual).AsCollection();
            var expectedItems = ((IEnumerable)expected).AsCollection();
            if (actualItems.Count != expectedItems.Count) { throw new PrimitiveAssertFailedException(node, $"actual の要素数 {actualItems.Count} と expected の要素数 {expectedItems.Count} が等しくありません。", _message); }

            var itemType = targetType.GetEnumerableElementType();
            var actualIter = actualItems.GetEnumerator();
            var expectedIter = expectedItems.GetEnumerator();
            for (var i = 0; i < actualItems.Count; i++) {
                actualIter.MoveNext();
                expectedIter.MoveNext();

                // NOTE: 要素型が不明の場合は、要素のランタイム型にフォールバック。
                var itemTargetType = itemType ?? actualIter.Current?.GetType();
                AssertIs(new AssertNode(i.ToString(), itemTargetType, actualIter.Current, expectedIter.Current, node));
            }
            if (targetType.IsSystemCollection()) { return true; }

            return false;
        }

        private bool TryCompositeAssertIs(Type targetType, object actual, object expected, AssertNode node) {
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            var actualType = actual.GetType();
            var expectedType = expected.GetType();
            if (targetType.IsAssignableFrom(actualType)) {
                actualType = targetType;
            }
            if (targetType.IsAssignableFrom(expectedType)) {
                expectedType = targetType;
            }

            // 参照の比較
            if (ReferenceEquals(actual, expected)) {
                if (!actualType.IsDuckImplemented(targetType)) { throw new PrimitiveAssertFailedException(node, "actual と expected は同じ参照ですが、ターゲット型に違反しています。", _message); }

                WriteLog(node, "actual と expected は同じ参照です。");
                return true;
            }

            // 各データ メンバーの比較
            foreach (var member in targetType.GetDataMembers()) {
                if (!actualType.TryGetDataMember(member.Name, out var actualMember)) { throw new PrimitiveAssertFailedException(node, $"actual にデータ メンバー {member.Name} が見つかりません。", _message); }
                if (!expectedType.TryGetDataMember(member.Name, out var expectedMember)) { throw new PrimitiveAssertFailedException(node, $"expected にデータ メンバー {member.Name} が見つかりません。", _message); }

                var actualMemberValue = actualMember.GetValue(actual);
                var expectedMemberValue = expectedMember.GetValue(expected);
                AssertIs(new AssertNode(member.Name, member.DataType, actualMemberValue, expectedMemberValue, node));
            }

            return false;
        }
    }
}
