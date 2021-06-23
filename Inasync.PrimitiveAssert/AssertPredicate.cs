namespace Inasync {

    /// <summary>
    /// <see cref="PrimitiveAssert"/> の <c>expected</c> に指定する事で任意のアサート条件が記述できるデリゲート。
    /// </summary>
    /// <param name="actual">検証対象の実値。</param>
    /// <returns><paramref name="actual"/> がアサートに成功した場合は <c>true</c>、失敗した場合は <c>false</c>。</returns>
    public delegate bool AssertPredicate(object? actual);

    /// <summary>
    /// <see cref="PrimitiveAssert"/> の <c>expected</c> に指定する事で任意のアサート条件が記述できるデリゲート。
    /// </summary>
    /// <typeparam name="T"><paramref name="actual"/> で受けたい型。キャストできない場合はアサートに失敗した扱いになる。</typeparam>
    /// <param name="actual">検証対象の実値。</param>
    /// <returns><paramref name="actual"/> がアサートに成功した場合は <c>true</c>、失敗した場合は <c>false</c>。</returns>
    public delegate bool AssertPredicate<T>(T actual);
}
