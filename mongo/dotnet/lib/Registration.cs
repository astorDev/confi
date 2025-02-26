using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persic;

namespace Confi;

public enum MongoLoadingMode
{
    CollectionWatching,
    LongPolling
}

public static class LoaderModeRegistration
{
    public static IServiceCollection AddMongoBackgroundConfigurationService(this IServiceCollection services, string documentId, MongoLoadingMode mode = MongoLoadingMode.CollectionWatching)
    {
        return services.AddMongoBackgroundConfigurationService(
            documentId,
            (loader) => mode == MongoLoadingMode.CollectionWatching
                ? new MongoBackgroundConfigurationWatcher(loader)
                : new MongoConfigurationPoller(loader)
        );
    }
}


public class MongoConfigurationBuilder(IServiceCollection services)
{
    public MongoConfigurationBuilder AddLoader(string configurationKey, MongoLoadingMode loadingMode = MongoLoadingMode.CollectionWatching)
    {
        services.AddMongoBackgroundConfigurationService(configurationKey, loadingMode);
        return this;
    }
}

public static class MongoConfigurationExtensions
{
    public static IHostApplicationBuilder AddMongoConfiguration(
        this IHostApplicationBuilder builder,
        Action<MongoConfigurationBuilder> configure,
        string configsCollectionName = "configs"
        )
    {
        builder.Configuration.AddBackgroundStore(MongoBackgroundConfigurationLoader.Key);

        builder.Services.AddBackgroundConfigurationStores();

        builder.Services.AddMongoCollection<ConfigurationRecord>(configsCollectionName);

        var loadersBuilder = new MongoConfigurationBuilder(builder.Services);
        configure(loadersBuilder);

        return builder;
    }
}