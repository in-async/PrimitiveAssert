using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.Tests {

    [TestClass]
    public class UsageTests {

        [ClassInitialize]
        public static void Initialize(TestContext context) {
            PrimitiveAssert.ConsoleLogging = true;
        }

        [TestMethod]
        public void Usage1() {
            var query = new FakeAccountQuery();
            var actual = query.Find(123);

            actual.AssertIs(new {
                AccountId = 123,
                Name = "John Smith",
                CreatedAt = new DateTime(2020, 3, 9, 8, 15, 0),
                Tags = new[] { "Foo", "Bar" },
            });
        }

        [TestMethod]
        public void Usage2() {
            var query = new FakeAccountQuery();
            var actual = query.Find(123);

            actual.AssertIs(new StubAccount {
                AccountId = 123,
                Name = "John Smith",
                CreatedAt = new DateTime(2020, 3, 9, 8, 15, 0),
                Tags = new[] { "Foo", "Bar" },
            });
        }

        [TestMethod]
        [ExpectedException(typeof(PrimitiveAssertFailedException))]
        public void Usage3() {
            var fooBar = new FooBar {
                Foo = "Foo1",
                Bar = "Bar2",
            };

            // IFoo として等値アサートしたいが、これだと PrimitiveAssertFailedException がスローされる
            fooBar.AssertIs(new {
                Foo = "Foo1",
            });
        }

        [TestMethod]
        public void Usage4() {
            var fooBar = new FooBar {
                Foo = "Foo1",
                Bar = "Bar2",
            };

            // IFoo として等値アサートされる。PrimitiveAssertFailedException はスローされない
            fooBar.AssertIs<IFoo>(new {
                Foo = "Foo1",
            });
            // or
            fooBar.AssertIs(typeof(IFoo), new {
                Foo = "Foo1",
            });
        }

        [TestMethod]
        public void Usage5() {
            IFoo fooBar = new FooBar {
                Foo = "Foo1",
                Bar = "Bar2",
            };

            // fooBar.AssertIs<IFoo> と同じ
            fooBar.AssertIs(new {
                Foo = "Foo1",
            });
        }

        [TestMethod]
        public void Usage6() {
            var query = new FakeAccountQuery();
            var actual = query.Find(123);

            var targetType = new {
                Name = default(string),
            }.GetType();

            // Name プロパティのみを等値アサートの対象としている
            actual.AssertIs(targetType, new {
                Name = "John Smith",
            });
        }

        #region Helpers

        public interface IAccount {
            int AccountId { get; }
            string Name { get; }
            DateTime CreatedAt { get; }
            IReadOnlyCollection<string> Tags { get; }
        }

        public class FakeAccountQuery {

            public IAccount Find(int accountId) {
                return new Account {
                    AccountId = accountId,
                    Name = "John Smith",
                    CreatedAt = new DateTime(2020, 3, 9, 8, 15, 0),
                    Tags = new[] { "Foo", "Bar" },
                };
            }

            private sealed class Account : IAccount {
                public int AccountId { get; set; }
                public string Name { get; set; }
                public DateTime CreatedAt { get; set; }
                public IReadOnlyCollection<string> Tags { get; set; }
            }
        }

        private class StubAccount : IAccount {
            public int AccountId { get; set; }
            public string Name { get; set; }
            public DateTime CreatedAt { get; set; }
            public IReadOnlyCollection<string> Tags { get; set; }
        }

        private interface IFoo {
            string Foo { get; }
        }

        private interface IBar {
            string Bar { get; }
        }

        private class FooBar : IFoo, IBar {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        #endregion Helpers
    }
}
