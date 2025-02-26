using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persic;

namespace Confi;

public enum MongoReadingMode
{
    CollectionWatching,
    LongPolling
}

public static class LoaderModeRegistration
{
    public static IServiceCollection AddMongoBackgroundConfigurationService(this IServiceCollection services, string documentId, MongoReadingMode mode = MongoReadingMode.CollectionWatching)
    {
        return services.AddSingleton<IHostedService>(sp => {
            var loaderFactory = sp.GetRequiredService<MongoConfigurationLoader.Factory>();
            var loader = loaderFactory.GetLoader(documentId);
            return mode == MongoReadingMode.CollectionWatching
                ? new MongoBackgroundConfigurationWatcher(loader)
                : new MongoConfigurationPoller(loader);
        });
    }
}

public class MongoConfigurationBuilder(IServiceCollection services)
{
    public MongoConfigurationBuilder AddLoader(string configurationKey, MongoReadingMode loadingMode = MongoReadingMode.CollectionWatching)
    {
        services.AddMongoBackgroundConfigurationService(configurationKey, loadingMode);
        return this;
    }
}

public static class MongoConfigurationExtensions
{
    public static IHostApplicationBuilder AddMongoConfiguration(
        this IHostApplicationBuilder builder,
        string documentId,
        string configsCollectionName = "configs"
    )
    {
        builder.Configuration.AddBackgroundStore(MongoConfigurationLoader.Key);

        builder.Services.AddBackgroundConfigurationStores();
        builder.Services.AddMongoCollection<ConfigurationRecord>(configsCollectionName);
        builder.Services.AddSingleton<MongoConfigurationLoader.Factory>();
        
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
        builder.Configuration.AddBackgroundStore(MongoConfigurationLoader.Key);

        builder.Services.AddBackgroundConfigurationStores();
        builder.Services.AddMongoCollection<ConfigurationRecord>(configsCollectionName);
        builder.Services.AddSingleton<MongoConfigurationLoader.Factory>();
        
        builder.Services.AddSingleton<IHostedService>(sp => {
            var loader = sp.GetRequiredService<MongoConfigurationLoader.Factory>().GetLoader(documentId);   
            return mode == MongoReadingMode.CollectionWatching
                ? new MongoBackgroundConfigurationWatcher(loader)
                : new MongoConfigurationPoller(loader);
        });

        return builder;
    }

    public static IHostApplicationBuilder AddMongoConfiguration(
        this IHostApplicationBuilder builder,
        Action<MongoConfigurationBuilder> configure,
        string configsCollectionName = "configs"
        )
    {
        builder.Configuration.AddBackgroundStore(MongoConfigurationLoader.Key);

        builder.Services.AddBackgroundConfigurationStores();
        builder.Services.AddMongoCollection<ConfigurationRecord>(configsCollectionName);
        builder.Services.AddSingleton<MongoConfigurationLoader.Factory>();

        var loadersBuilder = new MongoConfigurationBuilder(builder.Services);
        configure(loadersBuilder);

        return builder;
    }
}