using Com.Ctrip.Framework.Apollo.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace HamsterWoods.Silo;

public static class HamsterWoodsHostBuilderExtensions
{
    public static IHostBuilder UseApolloForConfigureHostBuilder(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration((_, builder) => { builder.AddJsonFile("appsettings.apollo.json"); })
            .ConfigureAppConfiguration((context, builder) =>
            {
                if (!context.Configuration.GetValue<bool>("IsApolloEnabled", false))
                {
                    return;
                }

                //To display the Apollo console logs 
#if DEBUG
                LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace);
#endif

                builder.AddApollo(builder.Build().GetSection("apollo"));
            })
            .ConfigureGloballySharedLog(false);
    }

    private static IHostBuilder ConfigureGloballySharedLog(this IHostBuilder hostBuilder, bool configureService)
    {
        return configureService
            ? hostBuilder.ConfigureServices((context, _) =>
            {
                ConfigureGloballySharedLog(context.Configuration);
                SetGloballySharedLoggerForApolloLogs();
            })
            : hostBuilder.ConfigureAppConfiguration((context, _) =>
            {
                ConfigureGloballySharedLog(context.Configuration);
                SetGloballySharedLoggerForApolloLogs();
            });
    }

    private static void ConfigureGloballySharedLog(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(configuration)
#if DEBUG
            .WriteTo.Async(c => c.Console())
#endif
            .CreateLogger();
    }

    private static void SetGloballySharedLoggerForApolloLogs()
    {
        LogManager.LogFactory = name => (level, message, exception) =>
        {
            var messageTemplate = $"{name} {message}";
            switch (level)
            {
                case LogLevel.Information:
                    Log.Information(messageTemplate);
                    break;
                case LogLevel.Warning:
                    Log.Warning(messageTemplate);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    if (exception == null)
                    {
                        Log.Error(messageTemplate);
                    }
                    else
                    {
                        Log.Error(exception, messageTemplate);
                    }

                    break;
                case LogLevel.Trace:
                case LogLevel.Debug:
                default:
                    Log.Debug(messageTemplate);
                    break;
            }
        };
    }

    public static IHostBuilder UseApolloForHostBuilder(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration((_, builder) => { builder.AddJsonFile("appsettings.apollo.json"); })
            .ConfigureAppConfiguration((_, builder) =>
            {
                if (!builder.Build().GetValue<bool>("IsApolloEnabled", false))
                {
                    return;
                }
                //To display the Apollo console logs 
#if DEBUG
                LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace);
#endif
                builder.AddApollo(builder.Build().GetSection("apollo"));
            })
            .ConfigureGloballySharedLog(true);
    }
}