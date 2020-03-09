# PrimitiveAssert
[![Build status](https://ci.appveyor.com/api/projects/status/ymgqfchxxgjple1j/branch/master?svg=true)](https://ci.appveyor.com/project/inasync/primitiveassert/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Inasync.PrimitiveAssert.svg)](https://www.nuget.org/packages/Inasync.PrimitiveAssert/)

***PrimitiveAssert*** は対象をプリミティブ データ型に分解してアサートするシンプルなライブラリです。


## Target Frameworks
- .NET Standard 2.0
- .NET Framework 4.6.1

## Description
このライブラリは、任意のデータ型に対して統一された API によって等値アサートを行う事を目的としています。

基本的な使い方は次の通りです:
```cs
actual.AssertIs(expected);
```

`PrimitiveAssert.AssertIs()` 拡張メソッドは、actual と expected の public かつ instance なデータ メンバーを等値アサート可能な最小単位 "プリミティブ データ" に分解し、比較します。

例えば、下記のようなデータ型 `IAccount` を返すメソッドをテストする場合:
```cs
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
            Tags = new []{ "Foo", "Bar" },
        };
    }

    private sealed class Account : IAccount {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt  { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
    }
}
```

次のように等値アサートが書けます:
```cs
var query = new FakeAccountQuery();
var actual = query.Find(123);

actual.AssertIs(new {
    AccountId = 123,
    Name = "John Smith",
    CreatedAt = new DateTime(2020, 3, 9, 8, 15, 0),
    Tags = new []{ "Foo", "Bar" },
});
```

ここでは expected に匿名型を用いましたが、もちろん `IAccount` を継承した型を使用する事もできます:
```cs
var query = new FakeAccountQuery();
var actual = query.Find(123);

actual.AssertIs(new StubAccount {
    AccountId = 123,
    Name = "John Smith",
    CreatedAt = new DateTime(2020, 3, 9, 8, 15, 0),
    Tags = new[] { "Foo", "Bar" },
});

...

class StubAccount : IAccount {
    public int AccountId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyCollection<string> Tags { get; set; }
}
```

どのようにアサートが行われているか、その詳細なログが必要な場合は
```cs
PrimitiveAssert.ConsoleLogging = true;
```
とする事で、次のようなログがコンソールに出力されます:
```
{
      path: ./AccountId:Int32
    target: System.Int32
    actual: 123
  expected: 123
}
{
      path: ./Name:String
    target: System.String
    actual: John Smith
  expected: John Smith
}
{
      path: ./CreatedAt:DateTime
    target: System.DateTime
    actual: 2020/03/09 8:15:00
  expected: 2020/03/09 8:15:00
}
{
      path: ./Tags:IReadOnlyCollection`1/0:String
    target: System.String
    actual: Foo
  expected: Foo
}
{
      path: ./Tags:IReadOnlyCollection`1/1:String
    target: System.String
    actual: Bar
  expected: Bar
}
```

### ターゲット型
比較対象となるデータ メンバーは、public かつ instnace なプロパティ及びフィールドが選ばれますが、時には比較対象から外したいデータ メンバーがあるかも知れません。
```cs
interface IFoo {
    string Foo { get; }
}
interface IBar {
    string Bar { get; }
}
class FooBar : IFoo, IBar {
    public string Foo { get; set; }
    public string Bar { get; set; }
}

...

var fooBar = new FooBar {
    Foo = "Foo1",
    Bar = "Bar2",
};

// IFoo として等値アサートしたいが、これだと PrimitiveAssertFailedException がスローされる
fooBar.AssertIs(new {
    Foo = "Foo1",
});
```

そのような場合はターゲット型を明示する事で対応が可能です:
```cs
// IFoo として等値アサートされる。PrimitiveAssertFailedException はスローされない
fooBar.AssertIs<IFoo>(new {
    Foo = "Foo1",
});
// or
fooBar.AssertIs(typeof(IFoo), new {
    Foo = "Foo1",
});
```

ターゲット型省略時は、actual の変数型がターゲット型となります。従って以下の記述でも `IFoo` の等値アサートは可能です:
```cs
IFoo fooBar = new FooBar {
    Foo = "Foo1",
    Bar = "Bar2",
};

// fooBar.AssertIs<IFoo> と同じ
fooBar.AssertIs(new {
    Foo = "Foo1",
});
```

また、actual はターゲット型を実装してる必要はなく、ダック タイピング的にデータ メンバーを備えていれば問題ありません。

例えば匿名型を使用して、任意のデータ メンバーのみを等値アサートの対象とする荒業もやろうと思えばできます:
```cs
var query = new FakeAccountQuery();
var actual = query.Find(123);

var targetType = new {
    Name = default(string),
}.GetType();

// Name プロパティのみを等値アサートの対象としている
actual.AssertIs(targetType, new {
    Name = "John Smith",
});
```
