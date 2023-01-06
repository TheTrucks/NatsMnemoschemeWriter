using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NatsMnemoschemeWriter.Impl;
using NatsMnemoschemeWriter.Interfaces;

namespace NatsMnemoschemeWriter
{
    internal class JRunner : IHostedService
    {
        private readonly IDataWriterManager<TestDataWriter> _writerManager;
        private readonly INatsDataInput<float> _dataInput;
        private readonly INatsOutput _output;
        private int[] _params = null!;
        private readonly DbWorker _db;
        private Random _rand = Random.Shared;

        private Task[] TickingStuff = new Task[4];

        public JRunner(IDataWriterManager<TestDataWriter> writerFactory, INatsDataInput<float> dataInput, INatsOutput output, DbWorker db)
        {
            _writerManager = writerFactory;
            _output = output;
            _db = db;
            _dataInput = dataInput;
        }

        private async Task Initialize(int[] paramIds)
        {
            await _writerManager.Initialize();
            foreach (var param in paramIds)
            {
                _writerManager.CreateInstance(param);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _output.Info("Worker starting...");
            await _output.Info("Getting params...");
            _params = await _db.GetAllParams();
            await Initialize(_params);
            await _output.Info($"Got {_params.Length} params");
            await _output.Info("Spawning Tasks...");
            TaskSpawner(cancellationToken);
            await _output.Info("Tasks spawned");
            await _output.Info("Worker started");
        }

        private void TaskSpawner(CancellationToken ct)
        {
            for (int i = 0; i <= 3; i++)
            {
                TickingStuff[i] = Task.Factory.StartNew(ord => WorkUnit(ord), i, ct);
            }
        }

        private async Task WorkUnit(object? order)
        {
            if (order is null)
                throw new ArgumentNullException();

            int taskOrder = (int)order;
            int limit = (int)Math.Floor(_params.Length / 4d);
            while (true) 
            {
                await Task.Delay(_rand.Next(5, 30) * 100);
                for (int i = limit * taskOrder; i < limit + limit * taskOrder; i++)
                {
                    if (i < _params.Length)
                        await _writerManager
                            .GetWriter(_params[i])
                            .SendDataAsync(new TestDataStruct 
                            { 
                                ParameterId = _params[i], 
                                Value = _dataInput.NextValue() 
                            });
                }
                await _output.Info($"Task with order {taskOrder} fired");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _output.Info("Worker stopping...");
            _writerManager.Dispose();
            _db.Dispose();
            await _output.Info("Worker stopped");
        }
    }
}
