using System;
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
                        .Act(() => x.AssertIs(y))
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
    }
}
