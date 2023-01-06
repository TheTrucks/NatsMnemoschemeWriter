namespace NatsMnemoschemeWriter.Interfaces
{
    public interface IAsyncNatsDataInput<TResult> : IDisposable
    {
        public IAsyncEnumerable<TResult> NextValue();
    }

    public interface INatsDataInput<TResult>
    {
        public TResult NextValue();
    }
}
