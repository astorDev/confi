using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persic;

namespace Confi;

public static class MongoConfigurationExtensions
{
    public static IHostApplicationBuilder AddMongoConfiguration(
        this IHostApplicationBuilder builder,
        string documentId,
        string configsCollectionName = "configs"
    )
    {
        builder.Services.AddMongoCollection<ConfigurationRecord>(configsCollectionName);
        builder.Services.AddSingleton<MongoConfigurationLoader.Factory>();

        builder.Configuration.AddBackgroundStore(MongoConfigurationLoader.Key(documentId));
        builder.Services.AddBackgroundConfigurationStores();
        builder.Services.AddSingleton<IHostedService>(sp => {
            var loader = sp.GetRequiredService<MongoConfigurationLoader.Factory>().GetLoader(documentId);   
            return new MongoBackgroundConfigurationWatcher(loader);
        });

        return builder;
    }

    public static IHostApplicationBuilder AddMongoConfiguration(
        this IHostApplicationBuilder builder,
        string documentId,
        MongoReadingMode mode,
        string configsCollectionName = "configs"
    )
    {
        builder.Services.AddMongoCollection<ConfigurationRecord>(configsCollectionName);
        builder.Services.AddSingleton<MongoConfigurationLoader.Factory>();

        builder.Configuration.AddBackgroundStore(MongoConfigurationLoader.Key(documentId));
        builder.Services.AddBackgroundConfigurationStores();
        builder.Services.AddSingleton<IHostedService>(sp => {
            var loader = sp.GetRequiredService<MongoConfigurationLoader.Factory>().GetLoader(documentId);   
            return mode == MongoReadingMode.CollectionWatching
                ? new MongoBackgroundConfigurationWatcher(loader)
                : new MongoConfigurationPoller(loader);
        });

        return builder;
    }
}

public enum MongoReadingMode
{
    CollectionWatching,
    LongPolling
}