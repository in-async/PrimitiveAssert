using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.Tests {

    [TestClass]
    public class PrimitiveSerializerTests {

        [TestMethod]
        public void ToPrimitiveString_Usage() {
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
                StringType = typeof(string),
            };

            Console.WriteLine(x.ToPrimitiveString(indented: false));
            Console.WriteLine(x.ToPrimitiveString(indented: true));
        }
    }
}
