using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NatsMnemoschemeWriter.Impl;
using NatsMnemoschemeWriter.Interfaces;

namespace NatsMnemoschemeWriter;
internal class Program
{
    static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("settings.json")
            .Build();
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(servs => ConfigureServices(servs, config));
        var host = builder.Build();

        await host.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.Configure<TestDataWriterOptions>(config.GetSection("NatsOpts"));
        services.Configure<DbOptions>(config.GetSection("PsqlConnect"));
        services.AddSingleton<DbWorker>();
        services.AddSingleton<INatsOutput, ConsoleNatsOutput>();
        services.AddSingleton<IDataWriterManager<TestDataWriter>, DataWriterManager>();
        services.AddSingleton<INatsDataInput<float>, TestDataInputRand>();
        services.AddHostedService<JRunner>();
    }
}