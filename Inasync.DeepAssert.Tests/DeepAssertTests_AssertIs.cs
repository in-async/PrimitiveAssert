using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.Tests {

    [TestClass]
    public class DeepAssertTests_AssertIs {

        private static Action TestCase(int testNo, object? x, object? y, Type? expectedException = null) => () => {
            TestAA
                .Act(() => x.AssertIs(y, message: $"No.{testNo}_a"))
                .Assert(expectedException, message: $"No.{testNo}_a");

            TestAA
                .Act(() => y.AssertIs(x, message: $"No.{testNo}_b"))
                .Assert(expectedException, message: $"No.{testNo}_b");
        };

        [TestMethod]
        public void AssertIs_Null() {
            new[] {
                TestCase( 0, null, null      ),
                TestCase( 1, null, (int?)null),
                TestCase( 2, null, 1         , expectedException: typeof(AssertIsFailedException)),
                TestCase( 3, null, ""        , expectedException: typeof(AssertIsFailedException)),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Int() {
            new[] {
                TestCase( 0, 1, 1      ),
                TestCase( 1, 1, (int?)1),
                TestCase( 2, 1, (byte)1),
                TestCase( 3, 1, 1L     ),
                TestCase( 4, 1, 1M     ),
                TestCase( 5, 1, 1D     ),
                TestCase( 6, 1, ""     , expectedException: typeof(AssertIsFailedException)),
                TestCase( 7, 1, 1.1    , expectedException: typeof(AssertIsFailedException)),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Double() {
            new[] {
                TestCase( 0, 1d, 1d        ),
                TestCase( 1, 1d, 1.0d      ),
                TestCase( 2, 1d, (double?)1),
                TestCase( 3, 1d, 1         ),
                TestCase( 4, 1d, ""        , expectedException: typeof(AssertIsFailedException)),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Decimal() {
            new[] {
                TestCase( 0, 1m, 1m         ),
                TestCase( 1, 1m, 1.0m       ),
                TestCase( 2, 1m, (decimal?)1),
                TestCase( 3, 1m, 1          ),
                TestCase( 4, 1m, ""         , expectedException: typeof(AssertIsFailedException)),
                TestCase( 5, 1m, 1.1        , expectedException: typeof(AssertIsFailedException)),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Guid() {
            var guidStr1 = "15b63bc6-9876-4e07-8400-f06daf3e4212";
            var guidStr2 = "25b63bc6-9876-4e07-8400-f06daf3e4212";
            var guid = Guid.Parse(guidStr1);
            new[] {
                TestCase( 0, guid, guid                ),
                TestCase( 1, guid, Guid.Parse(guidStr1)),
                TestCase( 2, guid, Guid.Parse(guidStr2), expectedException: typeof(AssertIsFailedException)),
                TestCase( 3, guid, guidStr1            , expectedException: typeof(AssertIsFailedException)),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_UserObject() {
            var actual = new {
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
                },
                Rank = 'A',
                Remarks = (string?)null,
                Params1 = (foo: 1, bar: "bar"),
                Params2 = (ValueTuple<int, string>?)null,
                Params3 = Tuple.Create(1, "bar"),
                LastError = typeof(ApplicationException),
            };
            var expected = new {
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
                },
                Rank = 'A',
                Remarks = (string?)null,
                Params1 = (foo: 1, bar: "bar"),
                Params2 = (ValueTuple<int, string>?)null,
                Params3 = Tuple.Create(1, "bar"),
                LastError = typeof(ApplicationException),
            };

            TestCase(0, actual, expected)();
        }
    }
}
