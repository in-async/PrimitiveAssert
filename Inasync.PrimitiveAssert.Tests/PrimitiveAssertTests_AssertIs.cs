using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.Tests {

    [TestClass]
    public class PrimitiveAssertTests_AssertIs {

        private static Action TestCase(int testNo, Type? target, object? x, object? y, Type? expectedException = null) => () => {
            TestAA
                .Act(() => x.AssertIs(target, y, message: $"No.{testNo}_a"))
                .Assert(expectedException, message: $"No.{testNo}_a");

            TestAA
                .Act(() => y.AssertIs(target, x, message: $"No.{testNo}_b"))
                .Assert(expectedException, message: $"No.{testNo}_b");
        };

        [TestMethod]
        public void AssertIs_Null() {
            new[] {
                TestCase( 0, target: null          , x: null, y: null),
                TestCase( 1, target: null          , x: null, y: 1   , expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。null 型には null のみが許容される。
                TestCase( 2, target: null          , x: 1   , y: null, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。null 型には null のみが許容される。
                TestCase( 3, target: null          , x: 1   , y: 1   , expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。null 型には null のみが許容される。

                TestCase(10, target: typeof(int)   , x: null, y: null, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。int 型に null は割り当てられない。
                TestCase(11, target: typeof(int)   , x: null, y: 1   , expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。int 型に null は割り当てられない。
                TestCase(12, target: typeof(int)   , x: 1   , y: null, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。int 型に null は割り当てられない。
                TestCase(13, target: typeof(int)   , x: 1   , y: 1   ),

                TestCase(20, target: typeof(int?)  , x: null, y: null),
                TestCase(21, target: typeof(int?)  , x: null, y: 1   , expectedException: typeof(PrimitiveAssertFailedException)),  // 値の不一致。
                TestCase(22, target: typeof(int?)  , x: 1   , y: null, expectedException: typeof(PrimitiveAssertFailedException)),  // 値の不一致。
                TestCase(23, target: typeof(int?)  , x: 1   , y: 1   ),

                TestCase(30, target: typeof(string), x: null, y: null),
                TestCase(31, target: typeof(string), x: null, y: 1   , expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。string 型に int 値は割り当てられない。
                TestCase(32, target: typeof(string), x: 1   , y: null, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。string 型に int 値は割り当てられない。
                TestCase(33, target: typeof(string), x: 1   , y: 1   , expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。string 型に int 値は割り当てられない。
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Reference() {
            var obj = new object();
            new[] {
                TestCase( 0, target: null          , x: obj, y: obj, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。null 型には null のみが許容される。
                TestCase( 1, target: typeof(string), x: obj, y: obj, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。string 型に object 値は割り当てられない。
                TestCase( 2, target: typeof(object), x: obj, y: obj),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Numeric() {
            new[] {
                TestCase( 0, target: typeof(int)    , x: 1   , y: 1   ),
                TestCase( 1, target: typeof(int)    , x: 1   , y: 1.1m, expectedException: typeof(PrimitiveAssertFailedException)),  // 値の不一致。
                TestCase( 2, target: typeof(int)    , x: 1.1d, y: 1.1m),  // ターゲット型が int や double 等の数値型の場合は、小数を含む任意の数値を表す Numeric 型扱いとなる。
                TestCase( 3, target: typeof(int)    , x: 1   , y: ""  , expectedException: typeof(PrimitiveAssertFailedException)),  // 型の不一致。
                TestCase(10, target: typeof(int?)   , x: 1   , y: 1   ),  // ターゲット型が数値型の Nullable の場合も Numeric 型扱い。
                TestCase(20, target: typeof(double) , x: 1   , y: 1.0m),
                TestCase(30, target: typeof(decimal), x: 1m  , y: 1.0m),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_PrimitiveData() {
            var guid1 = new Guid("15b63bc6-9876-4e07-8400-f06daf3e4212");
            var guid2 = new Guid("25b63bc6-9876-4e07-8400-f06daf3e4212");
            var dummy = new DummyStruct();
            new[] {
                TestCase( 0, target: typeof(Guid)  , x: guid1, y: guid1),
                TestCase( 1, target: typeof(Guid)  , x: guid1, y: dummy, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。DummyStruct 型は基本データ型ではない。
                TestCase( 2, target: typeof(Guid)  , x: dummy, y: guid1, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。DummyStruct 型は基本データ型ではない。
                TestCase( 3, target: typeof(Guid)  , x: guid1, y: guid2, expectedException: typeof(PrimitiveAssertFailedException)),  // 値の不一致。
                TestCase(10, target: typeof(string), x: guid1, y: guid1, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。string 型に Guid 値は割り当てられない。
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_Collection() {
            var countType = new { Count = 0 }.GetType();

            new[] {
                TestCase( 0, target: typeof(IEnumerable), x: new[]{1,2}, y: new[]{1,2}),
                TestCase( 1, target: typeof(IEnumerable), x: new[]{1,2}, y: new[]{1  }, expectedException: typeof(PrimitiveAssertFailedException)),  // 要素数の不一致。
                TestCase( 2, target: typeof(IEnumerable), x: new[]{1,2}, y: new[]{1,3}, expectedException: typeof(PrimitiveAssertFailedException)),  // 値の不一致。

                TestCase(10, target: typeof(string)     , x: new[]{'a','b'}    , y: new[]{'a','b'}    , expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。string 型に char[] 値は割り当てられない。
                TestCase(11, target: typeof(IEnumerable), x: "ab"              , y:"ab"               ),
                TestCase(12, target: typeof(IEnumerable), x: new object[]{1,""}, y: new object[]{1,""}),

                TestCase(20, target: countType          , x: new int[]    {1,2}, y: new{Count=2}, expectedException: typeof(PrimitiveAssertFailedException)),  // ダック型の不一致。Array に Count プロパティはない (明示的実装は比較されない)
                TestCase(21, target: countType          , x: new List<int>{1,2}, y: new{Count=2}),
                TestCase(22, target: typeof(ICollection), x: new List<int>{1,2}, y: new{Count=2}, expectedException: typeof(PrimitiveAssertFailedException)),  // ターゲット型違反。ICollection 型に匿名値は割り当てられない。
                TestCase(23, target: typeof(ICollection), x: new List<int>{1,2}, y: new[]{1,2}  ),
            }.Invoke();
        }

        [TestMethod]
        public void AssertIs_CompositeData() {
            var dummy = DummyClass();
            var readOnlyFieldType = new { ReadOnlyField = 0 }.GetType();

            new[] {
                TestCase( 0, target: typeof(DummyClass), x:dummy, y:dummy               ),
                TestCase( 1, target: typeof(DummyClass), x:dummy, y:DummyClass()        ),
                TestCase( 2, target: typeof(DummyClass), x:dummy, y:new{ReadOnlyField=1}, expectedException: typeof(PrimitiveAssertFailedException)),
                TestCase(10, target: readOnlyFieldType , x:dummy, y:DummyClass()        ),
                TestCase(11, target: readOnlyFieldType , x:dummy, y:new{ReadOnlyField=1}),
            }.Invoke();

            static DummyClass DummyClass() => new DummyClass(readOnlyField: 1, readWriteField: 2, readOnlyProperty: 3, readWriteProperty: 4, writeProperty: 5);
        }

        [TestMethod]
        public void AssertIs_Usage() {
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

        #region Helper

        private readonly struct DummyStruct { }

        private class DummyClass {
            public readonly int ReadOnlyField;
            public int ReadWriteField;

            public DummyClass(int readOnlyField, int readWriteField, int readOnlyProperty, int readWriteProperty, int writeProperty) {
                ReadOnlyField = readOnlyField;
                ReadWriteField = readWriteField;
                ReadOnlyProperty = readOnlyProperty;
                ReadWriteProperty = readWriteProperty;
                WriteProperty = writeProperty;
            }

            public int ReadOnlyProperty { get; }
            public int ReadWriteProperty { get; set; }
            public int WriteProperty { set { } }
        }

        #endregion Helper
    }
}
