using Microsoft.Extensions.Options;
using NatsMnemoschemeWriter.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsMnemoschemeWriter
{
    internal sealed class DbOptions
    {
        public string ConnString { get; set; }
    }

    internal class DbWorker : IDisposable
    {
        private readonly NpgsqlConnection _conn;
        private readonly DbOptions _opts;
        private readonly INatsOutput _output;
        private bool disposed = false;

        public DbWorker(IOptions<DbOptions> opts, INatsOutput output)
        {
            _opts = opts.Value;
            _conn = new NpgsqlConnection(_opts.ConnString);
            _output = output;
        }

        private async Task<NpgsqlConnection> CheckConnection()
        {
            if (_conn.State != System.Data.ConnectionState.Open)
            {
                await _conn.OpenAsync();
                await _output.Info("Psql connection opened");
            }
            return _conn;
        }

        public async Task<int[]> GetAllParams()
        {
            await CheckConnection();
            using (var cmd = new NpgsqlCommand($"select treenodes_id from machinery.treenodes where type_id = 3", _conn))
            {
                List<int> result = new();
                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    await _output.Info("Psql queried");
                    while (await rdr.ReadAsync())
                    {
                        result.Add(rdr.GetInt32(0));
                    }
                }
                return result.ToArray();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                _conn.Dispose();
                disposed = true;
            }
        }
    }
}
