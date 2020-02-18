using System;

namespace Inasync {

    internal sealed class AssertNode {

        public AssertNode(string name, Type? targetType, object? actual, object? expected, AssertNode? parent) {
            Name = name;
            TargetType = targetType;
            Actual = actual;
            Expected = expected;
            Parent = parent;

            if (parent == null) {
                Path = ".";
            }
            else {
                Path = parent.Path + "/" + name;
            }
        }

        public string Name { get; }
        public Type? TargetType { get; }
        public object? Actual { get; }
        public object? Expected { get; }
        public AssertNode? Parent { get; }
        public string Path { get; }

        public override string ToString() => $@"{{
      path: {Path}
    target: {TargetType?.FullName ?? "(null)"}
    actual: {Actual ?? "(null)"}
  expected: {Expected ?? "(null)"}
}}";
    }
}
