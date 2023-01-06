using AlterNats;
using NatsMnemoschemeWriter.Interfaces;

namespace NatsMnemoschemeWriter.Impl
{

    internal sealed class TestDataWriter : IDataWriter<TestDataStruct>
    {
        private readonly NatsConnection _nconn;
        private readonly INatsOutput _output;

        private readonly int _pid;
        private bool disposed = false;

        public TestDataWriter(NatsConnection nconn, int pid, INatsOutput output)
        {
            _nconn = nconn;
            _pid = pid;
            _output = output;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }
        }

        public async Task SendDataAsync(TestDataStruct input)
        {
            await _nconn.PublishAsync($"mach.params.{_pid}", input);
        }
    }
}
