using HamsterWoods.Silo.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace HamsterWoods.Silo;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting HamsterWoods.Silo.");
            var podIp = Environment.GetEnvironmentVariable("POD_IP");
            var orleansClusterId = Environment.GetEnvironmentVariable("ORLEANS_CLUSTER_ID");
            var orleansServiceId = Environment.GetEnvironmentVariable("ORLEANS_SERVICE_ID");
            Log.Information("podIp: {podIp}, orleansClusterId:{orleansClusterId}, orleansServiceId:{orleansServiceId}",
                podIp ?? "-", orleansClusterId ?? "-", orleansServiceId ?? "-");
            
            await CreateHostBuilder(args).RunConsoleAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    internal static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .InitAppConfiguration(true)
            .UseApolloForHostBuilder()
            .ConfigureServices((hostcontext, services) => { services.AddApplication<HamsterWoodsOrleansSiloModule>(); })
            .UseOrleansSnapshot()
            .UseAutofac()
            .UseSerilog();
}