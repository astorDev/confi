using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Confi;

public static class AppBuildingExtensions
{
    public static IConfigurationBuilder AddBackgroundStore(this IConfigurationBuilder configuration, ConfigurationBackgroundStore store)
    {
        return configuration.Add(new ConfigurationBackgroundStore.Source(store));
    }

    public static IConfigurationBuilder AddBackgroundStore(this IConfigurationBuilder configuration)
    {
        return configuration.AddBackgroundStore(ConfigurationBackgroundStore.Instance);
    }

    public static IConfigurationBuilder AddBackgroundStore(this IConfigurationBuilder configuration, string key)
    {
        return configuration.AddBackgroundStore(ConfigurationBackgroundStore.GetInstance(key));
    }

    public static IServiceCollection AddBackgroundConfigurationStores(this IServiceCollection services)
    {
        services.AddSingleton(ConfigurationBackgroundStore.Instance);
        services.AddSingleton<ConfigurationBackgroundStore.Factory>();
        return services;
    }

    public static IHostApplicationBuilder AddBackgroundConfiguration<THostedService>(this IHostApplicationBuilder builder, string key)
        where THostedService : class, IHostedService
    {
        builder.Configuration.AddBackgroundStore(key);
        builder.Services.AddBackgroundConfigurationStores();
        builder.Services.AddSingleton<IHostedService, THostedService>();
        return builder;
    }

    public static IHostApplicationBuilder AddBackgroundConfiguration<THostedService>(
        this IHostApplicationBuilder builder, 
        string key,
        Func<IServiceProvider, THostedService> factory
        )
        where THostedService : class, IHostedService
    {
        builder.Configuration.AddBackgroundStore(key);
        builder.Services.AddBackgroundConfigurationStores();
        builder.Services.AddSingleton<IHostedService>(factory);
        return builder;
    }
}
