using Microsoft.Extensions.Options;
using NatsMnemoschemeWriter.Interfaces;
using Npgsql;

namespace NatsMnemoschemeWriter.Impl
{
    internal sealed class TestDataInputRand : INatsDataInput<float>
    {
        private readonly Random _rand = Random.Shared;

        public float NextValue()
        {
            return _rand.Next(0, 99) + (float)(_rand.Next(10, 100_000_000) / 100_000_000.0);
        }
    }
}
