using AlterNats;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NatsMnemoschemeWriter.Interfaces;
using System.Collections.Concurrent;

namespace NatsMnemoschemeWriter.Impl
{
    internal sealed class DataWriterManager : IDataWriterManager<TestDataWriter>
    {
        private readonly IServiceProvider _sp;
        private readonly TestDataWriterOptions _options;
        private readonly INatsOutput _output;
        private readonly NatsConnection _nconn;
        private ConcurrentDictionary<int, TestDataWriter> _writers = new();
        private bool disposed = false;
        private bool init = false;

        public DataWriterManager(IServiceProvider sp)
        {
            _sp = sp;
            _options = sp.GetRequiredService<IOptions<TestDataWriterOptions>>().Value;
            _output = sp.GetRequiredService<INatsOutput>();
            _nconn = new NatsConnection(NatsOptions.Default with
            {
                Url = _options.Url,
                Serializer = new MessagePackNatsSerializer(),
                ConnectOptions = ConnectOptions.Default with
                {
                    Username = _options.User,
                    Password = _options.Password
                }
            });
        }

        public async Task Initialize()
        {
            try
            {
                await _nconn.ConnectAsync();
                await _output.Info("Connected");
            }
            catch (Exception exc)
            {
                await _output.Info(exc.ToString());
            }
            init = true;
        }

        public TestDataWriter CreateInstance(int ParamId)
        {
            if (!init)
                throw new Exception("Data writer factory isn't initialized");
            var writer = _writers.AddOrUpdate(ParamId, 
                val => new TestDataWriter(_nconn, val, _sp.GetRequiredService<INatsOutput>()),
                (eval, ewriter) =>
                {
                    ewriter.Dispose();
                    return new TestDataWriter(_nconn, eval, _sp.GetRequiredService<INatsOutput>());
                });
            return writer;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                foreach (var writer in _writers)
                    writer.Value.Dispose();
                _nconn.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                disposed = true;
            }
        }

        public TestDataWriter GetWriter(int ParamId)
        {
            return _writers[ParamId];
        }
    }

    internal class TestDataWriterOptions
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
