using System;
using AElf.Indexing.Elasticsearch.Options;
using HamsterWoods.Commons;
using Medallion.Threading;
using Medallion.Threading.Redis;
using HamsterWoods.EntityEventHandler.Core;
using HamsterWoods.Grains;
using HamsterWoods.MongoDb;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Providers.MongoDB.Configuration;
using StackExchange.Redis;
using Volo.Abp;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.Tokens;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler;

[DependsOn(typeof(AbpAutofacModule),
    typeof(HamsterWoodsPushMongoDbModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(HamsterWoodsEntityEventHandlerCoreModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpEventBusRabbitMqModule))]
public class HamsterWoodsEntityEventHandlerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        ConfigureTokenCleanupService();
        context.Services.AddHostedService<HamsterWoodsHostedService>();
        ConfigureCache(configuration);
        ConfigureEsIndexCreation();
        ConfigureDistributedLocking(context, configuration);
        ConfigureOrleans(context, configuration);
    }

    private static void ConfigureOrleans(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddSingleton<IClusterClient>(o =>
        {
            return new ClientBuilder()
                .ConfigureDefaults()
                .UseMongoDBClient(configuration["Orleans:MongoDBClient"])
                .UseMongoDBClustering(options =>
                {
                    options.DatabaseName = configuration["Orleans:DataBase"];
                    options.Strategy = MongoDBMembershipStrategy.SingleDocument;
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = configuration["Orleans:ClusterId"];
                    options.ServiceId = configuration["Orleans:ServiceId"];
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(HamsterWoodsGrainsModule).Assembly).WithReferences())
                .ConfigureLogging(builder => builder.AddProvider(o.GetService<ILoggerProvider>()))
                .Build();
        });
    }

    private void ConfigureDistributedLocking(
        ServiceConfigurationContext context,
        IConfiguration configuration)
    {
        context.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var connection = ConnectionMultiplexer
                .Connect(configuration["Redis:Configuration"]);
            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });
    }

    private void ConfigureCache(IConfiguration configuration)
    {
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "HamsterWoods:";
            options.GlobalCacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = CommonConstant.DefaultAbsoluteExpiration
            };
        });
    }
    
    private void ConfigureEsIndexCreation()
    {
        Configure<IndexCreateOption>(x => { x.AddModule(typeof(HamsterWoodsDomainModule)); });
    }

    private void ConfigureTokenCleanupService()
    {
        Configure<TokenCleanupOptions>(x => x.IsCleanupEnabled = false);
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        StartOrleans(context.ServiceProvider);
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        StopOrleans(context.ServiceProvider);
    }

    private static void StartOrleans(IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<IClusterClient>();
        AsyncHelper.RunSync(async () => await client.Connect());
    }

    private static void StopOrleans(IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<IClusterClient>();
        AsyncHelper.RunSync(client.Close);
    }
}