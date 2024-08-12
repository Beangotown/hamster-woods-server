using System;
using AElf.Indexing.Elasticsearch.Options;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using HamsterWoods.Commons;
using Medallion.Threading;
using Medallion.Threading.Redis;
using HamsterWoods.EntityEventHandler.Core;
using HamsterWoods.EntityEventHandler.Core.Services;
using HamsterWoods.EntityEventHandler.Core.Worker;
using HamsterWoods.Grains;
using HamsterWoods.MongoDb;
using HamsterWoods.Options;
using Hangfire.Mongo;
using Hangfire.Mongo.CosmosDB;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Orleans;
using Orleans.Configuration;
using Orleans.Providers.MongoDB.Configuration;
using StackExchange.Redis;
using Volo.Abp;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.Tokens;
using Volo.Abp.Threading;
using Hangfire;

namespace HamsterWoods.EntityEventHandler;

[DependsOn(typeof(AbpAutofacModule),
    typeof(HamsterWoodsPushMongoDbModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(HamsterWoodsEntityEventHandlerCoreModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpEventBusRabbitMqModule))]
public class HamsterWoodsEntityEventHandlerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        ConfigureTokenCleanupService();
        context.Services.AddHostedService<HamsterWoodsHostedService>();
        ConfigureCache(context, configuration);
        ConfigureEsIndexCreation();
        ConfigureDistributedLocking(context, configuration);
        ConfigureGraphQl(context, configuration);
        ConfigureOrleans(context, configuration);
        ConfigureHangfire(context, configuration);
        Configure<SyncPriceDataOptions>(configuration.GetSection("SyncPrice"));
        Configure<PointJobOptions>(configuration.GetSection("PointJob"));
    }

    private void ConfigureHangfire(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var mongoType = configuration["Hangfire:MongoType"];
        var connectionString = configuration["Hangfire:ConnectionString"];
        if (connectionString.IsNullOrEmpty()) return;

        if (mongoType.IsNullOrEmpty() ||
            mongoType.Equals(MongoType.MongoDb.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            context.Services.AddHangfire(x =>
            {
                x.UseMongoStorage(connectionString, new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    },
                    CheckConnection = true,
                    CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
                });
            });
        }
        else if (mongoType.Equals(MongoType.DocumentDb.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            context.Services.AddHangfire(config =>
            {
                var mongoUrlBuilder = new MongoUrlBuilder(connectionString);
                var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());
                var opt = new CosmosStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        BackupStrategy = new NoneMongoBackupStrategy(),
                        MigrationStrategy = new DropMongoMigrationStrategy(),
                    }
                };
                config.UseCosmosStorage(mongoClient, mongoUrlBuilder.DatabaseName, opt);
            });
        }

        context.Services.AddHangfireServer(opt =>
        {
            opt.SchedulePollingInterval = TimeSpan.FromMilliseconds(10000);
            opt.HeartbeatInterval = TimeSpan.FromMilliseconds(10000);
            opt.Queues = new[] { "default", "notDefault" };
        });
    }

    private void ConfigureGraphQl(ServiceConfigurationContext context,
        IConfiguration configuration)
    {
        context.Services.AddSingleton(new GraphQLHttpClient(configuration["GraphQL:Configuration"],
            new NewtonsoftJsonSerializer()));
        context.Services.AddScoped<IGraphQLClient>(sp => sp.GetRequiredService<GraphQLHttpClient>());
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

    private void ConfigureCache(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var multiplexer = ConnectionMultiplexer
            .Connect(configuration["Redis:Configuration"]);
        context.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "HamsterWoods:"; });
    }

    private void ConfigureEsIndexCreation()
    {
        Configure<IndexCreateOption>(x => { x.AddModule(typeof(HamsterWoodsDomainModule)); });
    }

    private void ConfigureTokenCleanupService()
    {
        Configure<TokenCleanupOptions>(x => x.IsCleanupEnabled = false);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // context.AddBackgroundWorkerAsync<SyncRankRecordWorker>();
        context.AddBackgroundWorkerAsync<SyncPriceWorker>();
        context.AddBackgroundWorkerAsync<SyncHopRecordWorker>();
        context.AddBackgroundWorkerAsync<CreateBatchSettleWorker>();

        InitRecurringJob(context.ServiceProvider);
        ConfigurationProvidersHelper.DisplayConfigurationProviders(context);
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        StartOrleans(context.ServiceProvider);
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        StopOrleans(context.ServiceProvider);
    }
    
    private static void InitRecurringJob(IServiceProvider serviceProvider)
    {
        var jobsService = serviceProvider.GetRequiredService<IInitJobsService>();
        jobsService.InitRecurringJob();
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