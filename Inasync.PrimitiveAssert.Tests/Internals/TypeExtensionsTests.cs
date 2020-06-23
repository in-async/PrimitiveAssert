using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.Tests {

    [TestClass]
    public class TypeExtensionsTests {

        [TestMethod]
        public void IsPrimitiveData() {
            Action TestCase(int testNo, Type type, bool expected) => () => {
                TestAA
                    .Act(() => type.IsPrimitiveData())
                    .Assert(expected, message: $"No.{testNo}");
            };

            new[] {
                TestCase( 0, typeof(bool)    , expected: true),
                TestCase( 1, typeof(char)    , expected: true),
                TestCase( 2, typeof(DateTime), expected: true),
                TestCase( 3, typeof(DateTimeOffset), expected: true),
                TestCase( 4, typeof(TimeSpan), expected: true),
                TestCase( 5, typeof(Guid)    , expected: true),
                TestCase( 6, typeof(StubEnum), expected: true),
                TestCase( 7, typeof(sbyte)   , expected: true),
                TestCase( 8, typeof(byte)    , expected: true),
                TestCase( 9, typeof(short)   , expected: true),
                TestCase(10, typeof(ushort)  , expected: true),
                TestCase(11, typeof(int)     , expected: true),
                TestCase(12, typeof(uint)    , expected: true),
                TestCase(13, typeof(long)    , expected: true),
                TestCase(14, typeof(ulong)   , expected: true),
                TestCase(15, typeof(float)   , expected: true),
                TestCase(16, typeof(double)  , expected: true),
                TestCase(17, typeof(decimal) , expected: true),

                TestCase(30, typeof(string)  , expected: true),
                TestCase(31, typeof(Uri)     , expected: true),

                TestCase(50, typeof(bool?)    , expected: true),
                TestCase(51, typeof(char?)    , expected: true),
                TestCase(52, typeof(DateTime?), expected: true),
                TestCase(53, typeof(DateTimeOffset?), expected: true),
                TestCase(54, typeof(TimeSpan?), expected: true),
                TestCase(55, typeof(Guid?)    , expected: true),
                TestCase(56, typeof(StubEnum?), expected: true),
                TestCase(57, typeof(sbyte?)   , expected: true),
                TestCase(58, typeof(byte?)    , expected: true),
                TestCase(59, typeof(short?)   , expected: true),
                TestCase(60, typeof(ushort?)  , expected: true),
                TestCase(61, typeof(int?)     , expected: true),
                TestCase(62, typeof(uint?)    , expected: true),
                TestCase(63, typeof(long?)    , expected: true),
                TestCase(64, typeof(ulong?)   , expected: true),
                TestCase(65, typeof(float?)   , expected: true),
                TestCase(66, typeof(double?)  , expected: true),
                TestCase(67, typeof(decimal?) , expected: true),

                TestCase(100, typeof(BigInteger)     , expected: false),
                TestCase(101, typeof(object)         , expected: false),
                TestCase(102, typeof(ValueType)      , expected: false),
                TestCase(103, typeof(Tuple<int>)     , expected: false),
                TestCase(104, typeof(ValueTuple<int>), expected: false),
                TestCase(105, typeof(Array)          , expected: false),
                TestCase(106, typeof(List<int>)      , expected: false),
                TestCase(107, typeof(ArraySegment<int>), expected: false),
                TestCase(108, typeof(Enum)           , expected: false),
                TestCase(109, typeof(Exception)      , expected: false),
                TestCase(110, typeof(IntPtr)         , expected: false),
                TestCase(111, typeof(UIntPtr)        , expected: false),
                TestCase(112, typeof(Type)           , expected: true ),
                TestCase(113, typeof(Version)        , expected: false),
            }.Invoke();
        }

        private enum StubEnum {
            Foo = 1,
        }
    }
}
