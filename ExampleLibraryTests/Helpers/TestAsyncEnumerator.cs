namespace ExampleLibraryTests.Helpers;

internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask DisposeAsync() => new ValueTask();
    public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(inner.MoveNext());
}