using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.Tests {

    [TestClass]
    public class IssueTests {

        [TestMethod]
        public void Issue15_Test() => Issue15.Test();

        private static class Issue15 {

            public static void Test() {
                IDerived x = new Derived { V0 = 0, V1 = 1 };

                Action TestCase(int testNo, object y, Type? expectedException = null) => () => {
                    TestAA
                        .Act(() => x.AssertIs(y))
                        .Assert(expectedException, message: $"No.{testNo}");
                };

                new[] {
                    TestCase( 0, y: new { V1 = 1 }, expectedException: typeof(PrimitiveAssertFailedException)),  // y が IDerived のデータメンバーを全て実装していないので失敗すべき。
                    TestCase( 1, y: new { V0 = 0, V1 = 1 }),
                }.Invoke();
            }

            private interface IBase {
                int V0 { get; }
            }

            private interface IDerived : IBase {
                int V1 { get; }
            }

            private class Derived : IDerived {
                public int V0 { get; set; }
                public int V1 { get; set; }
            }
        }

        [TestMethod]
        public void Issue15_2_Test() => Issue15_2.Test();

        private static class Issue15_2 {

            public static void Test() {
                var x = new FooBar(foo: 1, bar: 2, value: "foo bar", fooValue: "foo", barValue: "bar");

                static Action TestCase<T>(int testNo, T x, object y, Type? expectedException = null) => () => {
                    TestAA
                        .Act(() => x.AssertIs(y, $"No.{testNo}"))
                        .Assert(expectedException, message: $"No.{testNo}");
                };

                new[] {
                    TestCase( 0, (FooBar)x , y: new { Foo = 1, Bar = 2, Value = "foo bar" }),
                    TestCase( 1, (IFoo)x   , y: new { Foo = 1, Value = "foo" }),
                    TestCase( 2, (IBar)x   , y: new { Bar = 2, Value = "bar" }),
                    TestCase( 3, (IFooBar)x, y: new { Foo = 1, Bar = 2, Value = "foo bar" }, expectedException: typeof(ArgumentException)),  // CS0229 相当: IFoo.Value と IBar.Value 間があいまい
                    TestCase( 4, (IFooBar)x, y: new FooBar(foo: 1, bar: 2, value: "foo bar", fooValue: "foo", barValue: "bar"), expectedException: typeof(ArgumentException)),  // 実体が等価に見えても、ターゲット型 IFooBar の Value があいまいなまま
                    TestCase( 5, (IFooBar)x, y: x),  // 参照等価であれば、IFooBar.Value のあいまいさを解決する必要が無いので、成功
                }.Invoke();
            }

            private interface IFoo {
                int Foo { get; }
                string Value { get; }
            }

            private interface IBar {
                int Bar { get; }
                string Value { get; }
            }

            private interface IFooBar : IFoo, IBar { }

            private sealed class FooBar : IFooBar {
                private readonly string _fooValue;
                private readonly string _barValue;

                public FooBar(int foo, int bar, string value, string fooValue, string barValue) {
                    Foo = foo;
                    Bar = bar;
                    Value = value;
                    _fooValue = fooValue;
                    _barValue = barValue;
                }

                public int Foo { get; }
                public int Bar { get; }
                public string Value { get; }
                string IFoo.Value => _fooValue;
                string IBar.Value => _barValue;
            }
        }

        [TestMethod]
        public void Issue19_Test() {
            var actual = new {
                Foo = 1,
                Bar = 2,
            };

            TestAA.Act(() => actual.AssertIs(new { Foo = 1 })).Assert<PrimitiveAssertFailedException>();  // expected がターゲット型を満たしていないので失敗
            TestAA.Act(() => actual.AssertIs(new { Foo = 1, Bar = 2 })).Assert();  // actual と expected が完全一致してるので成功
            TestAA.Act(() => actual.AssertIs(new { Foo = 1, Bar = 2, Baz = 3 })).Assert<PrimitiveAssertFailedException>();  // expected がターゲット型と完全に一致していないので失敗
        }

        [TestMethod]
        public void Issue19_2_Test() => Issue19_2.Test();

        private static class Issue19_2 {

            public static void Test() {
                static Action TestCase(int testNo, Type targetType, object x, object y, Type? expectedException = null) => () => {
                    TestAA
                        .Act(() => x.AssertIs(targetType, y))
                        .Assert(expectedException, message: $"No.{testNo}");
                };

                var array = new[] { 1, 2, 3 };
                var custom = new CustomEnumerable<int>(new[] { 1, 2, 3 });
                new[] {
                    TestCase( 0, targetType: typeof(IEnumerable)          , x: array , y: custom, expectedException: typeof(PrimitiveAssertFailedException)),  // 失敗：ターゲット型と expected が一致していない
                    TestCase( 1, targetType: typeof(IEnumerable)          , x: custom, y: array ),  // 成功
                    TestCase( 2, targetType: typeof(CustomEnumerable<int>), x: array , y: custom, expectedException: typeof(PrimitiveAssertFailedException)),  // 失敗：actual をターゲット型にダックキャストできない
                    TestCase( 3, targetType: typeof(CustomEnumerable<int>), x: custom, y: array , expectedException: typeof(PrimitiveAssertFailedException)),  // 失敗：ターゲット型と expected が一致していない
                    TestCase( 4, targetType: typeof(CustomEnumerable<int>), x: custom, y: custom),  // 成功
                }.Invoke();
            }

            private class CustomEnumerable<T> : IEnumerable<T> {
                private readonly IEnumerable<T> _source;

                public CustomEnumerable(IEnumerable<T> source) {
                    _source = source ?? throw new ArgumentNullException(nameof(source));
                }

                public string? CustomValue { get; set; }

                public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => _source.GetEnumerator();
            }
        }

        [TestMethod]
        public void Issue20_Test() {
            var actual = new[] { 1, 3, 2 }.OrderBy(x => x);

            actual.AssertIs(new[] { 1, 2, 3 });
        }
    }
}
