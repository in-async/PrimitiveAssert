using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.Tests {

    [TestClass]
    public class DeepAssertTests_AssertIs {

        private static Action TestCase(int testNo, Type? target, object? x, object? y, Type? expectedException = null) => () => {
            TestAA
                .Act(() => DeepAssert.AssertIs(new DeepAssertArgs(target, x, y, path: ""), message: $"No.{testNo}_a"))
                .Assert(expectedException, message: $"No.{testNo}_a");

            TestAA
                .Act(() => DeepAssert.AssertIs(new DeepAssertArgs(target, y, x, path: ""), message: $"No.{testNo}_b"))
                .Assert(expectedException, message: $"No.{testNo}_b");
        };

        [TestMethod]
        public void AssertIs_Null() {
            new[] {
                TestCase( 0, target: null        , x: null, y: null),
                TestCase( 1, target: null        , x: null, y: 1   , expectedException: typeof(DeepAssertFailedException)),
                TestCase( 2, target: null        , x: 1   , y: 1   , expectedException: typeof(DeepAssertFailedException)),
                TestCase(10, target: typeof(int?), x: null, y: null),
                TestCase(11, target: typeof(int?), x: null, y: 1   , expectedException: typeof(DeepAssertFailedException)),
                TestCase(12, target: typeof(int?), x: 1   , y: 1   ),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Numeric() {
            new[] {
                TestCase( 0, target: typeof(int)    , x: 1   , y: 1   ),
                TestCase( 1, target: typeof(int)    , x: 1   , y: 1.1m, expectedException: typeof(DeepAssertFailedException)),
                TestCase( 2, target: typeof(int)    , x: 1.1d, y: 1.1m),
                TestCase( 3, target: typeof(int)    , x: 1   , y: ""  , expectedException: typeof(DeepAssertFailedException)),
                TestCase(10, target: typeof(double) , x: 1   , y: 1.0m),
                TestCase(20, target: typeof(decimal), x: 1m  , y: 1.0m),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_PrimitiveData() {
            var guidStr1 = "15b63bc6-9876-4e07-8400-f06daf3e4212";
            var guidStr2 = "25b63bc6-9876-4e07-8400-f06daf3e4212";
            var guid1 = Guid.Parse(guidStr1);
            new[] {
                TestCase( 0, target: typeof(Guid)  , x: guid1, y: Guid.Parse(guidStr1)),
                TestCase( 1, target: typeof(Guid)  , x: guid1, y: Guid.Parse(guidStr2), expectedException: typeof(DeepAssertFailedException)),
                TestCase( 2, target: typeof(Guid)  , x: guid1, y: guidStr1            , expectedException: typeof(DeepAssertFailedException)),
                TestCase(10, target: typeof(object), x: guid1, y: Guid.Parse(guidStr1)),
                TestCase(11, target: typeof(string), x: guid1, y: Guid.Parse(guidStr1), expectedException: typeof(DeepAssertFailedException)),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Collection() {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void AssertIs_CompositeData() {
            var x = new {
                AccountId = new Guid("f5b63bc6-9876-4e07-8400-f06daf3e4212"),
                FullName = "John Smith",
                Age = 20,
                Margin = 0.30m,
                CreatedAt = new DateTime(2020, 2, 13, 15, 56, 11),
                CreatedAtOffset = (DateTimeOffset)new DateTime(2020, 2, 13, 15, 56, 11),
                Enabled = true,
                Tags = new[] {
                    new{ Text = "Tag 1" },
                    new{ Text = "Tag 2" },
                    null,
                },
                Rank = 'A',
                Remarks = (string?)null,
                Params1 = (foo: 1, bar: "bar"),
                Params2 = (ValueTuple<int, string>?)null,
                Params3 = Tuple.Create(1, "bar"),
                LastError = new ApplicationException(),
            };
            var y = new {
                AccountId = new Guid("f5b63bc6-9876-4e07-8400-f06daf3e4212"),
                FullName = "John Smith",
                Age = 20,
                Margin = 0.3,
                CreatedAt = new DateTime(2020, 2, 13, 15, 56, 11),
                CreatedAtOffset = (DateTimeOffset)new DateTime(2020, 2, 13, 15, 56, 11),
                Enabled = true,
                Tags = new[] {
                    new{ Text = "Tag 1" },
                    new{ Text = "Tag 2" },
                    null,
                },
                Rank = 'A',
                Remarks = (string?)null,
                Params1 = (foo: 1, bar: "bar"),
                Params2 = (ValueTuple<int, string>?)null,
                Params3 = Tuple.Create(1, "bar"),
                LastError = new ApplicationException(),
            };

            TestCase(0, target: x.GetType(), x: x, y: y)();
        }
    }
}
