using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.Tests {

    [TestClass]
    public class NumericTests {

        [TestMethod]
        public void IsNumeric() {
            Action TestCase(int testNo, Type type, bool expected) => () => {
                TestAA
                    .Act(() => Numeric.IsNumeric(type))
                    .Assert(expected, message: $"No.{testNo}");
            };

            new[] {
                TestCase( 0, typeof(sbyte)  , expected: true),
                TestCase( 1, typeof(byte)   , expected: true),
                TestCase( 2, typeof(short)  , expected: true),
                TestCase( 3, typeof(ushort) , expected: true),
                TestCase( 4, typeof(int)    , expected: true),
                TestCase( 5, typeof(uint)   , expected: true),
                TestCase( 6, typeof(long)   , expected: true),
                TestCase( 7, typeof(ulong)  , expected: true),
                TestCase( 8, typeof(float)  , expected: true),
                TestCase( 9, typeof(double) , expected: true),
                TestCase(10, typeof(decimal), expected: true),

                TestCase(50, typeof(sbyte?)   , expected: true),
                TestCase(51, typeof(byte?)    , expected: true),
                TestCase(52, typeof(short?)   , expected: true),
                TestCase(53, typeof(ushort?)  , expected: true),
                TestCase(54, typeof(int?)     , expected: true),
                TestCase(55, typeof(uint?)    , expected: true),
                TestCase(56, typeof(long?)    , expected: true),
                TestCase(57, typeof(ulong?)   , expected: true),
                TestCase(58, typeof(float?)   , expected: true),
                TestCase(59, typeof(double?)  , expected: true),
                TestCase(60, typeof(decimal?) , expected: true),

                TestCase(100, typeof(BigInteger)     , expected: false),
                TestCase(101, typeof(object)         , expected: false),
                TestCase(102, typeof(ValueType)      , expected: false),
                TestCase(103, typeof(Tuple<int>)     , expected: false),
                TestCase(104, typeof(ValueTuple<int>), expected: false),
                TestCase(105, typeof(Enum)           , expected: false),
                TestCase(106, typeof(IntPtr)         , expected: false),
                TestCase(107, typeof(UIntPtr)        , expected: false),
                TestCase(108, typeof(Version)        , expected: false),
            }.Invoke();
        }

        private enum StubEnum {
            Foo = 1,
        }
    }
}
