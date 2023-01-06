namespace NatsMnemoschemeWriter.Interfaces
{
    internal interface IDataWriterManager<TType> : IDisposable
    {
        Task Initialize();
        TType CreateInstance(int ParamId);
        TType GetWriter(int ParamId);
    }
}