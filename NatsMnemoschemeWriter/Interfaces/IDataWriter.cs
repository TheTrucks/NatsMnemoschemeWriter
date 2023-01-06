namespace NatsMnemoschemeWriter.Interfaces
{
    internal interface IDataWriter<TType> : IDisposable
    {
        Task SendDataAsync(TType input);
    }
}
