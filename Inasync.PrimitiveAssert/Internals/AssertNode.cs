using System;
using Commons;

namespace Inasync {

    internal sealed class AssertNode {

        public AssertNode(string memberName, Type? targetType, object? actual, object? expected, AssertNode? parent) {
            MemberName = memberName;
            TargetType = targetType;
            Actual = actual;
            Expected = expected;
            Parent = parent;

            if (parent == null) {
                Path = "actual";
            }
            else {
                Path = parent.Path + "/" + memberName;
                if (targetType != null) {
                    Path += ":" + targetType.GetFriendlyName();
                }
            }
        }

        public string MemberName { get; }
        public Type? TargetType { get; }
        public object? Actual { get; }
        public object? Expected { get; }
        public AssertNode? Parent { get; }
        public string Path { get; }

        public override string ToString() => $@"{{
      path: {Path}
    target: {TargetType.ToPrimitiveString()}
    actual: {Actual.ToPrimitiveString()}
  expected: {Expected.ToPrimitiveString()}
}}";
    }
}
