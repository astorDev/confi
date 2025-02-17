using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Confi;

public class ConfigurationBackgroundStore
{
    public static ConfigurationBackgroundStore Instance { get; } = new();
    private Dictionary<string, object> data = new();

    private ConfigurationBackgroundStore(){}

    private List<Action<Dictionary<string, object>>> listeners = new();

    public void AddListener(Action<Dictionary<string, object>> listener)
    {
        listeners.Add(listener);
        listener(data);
    }

    public void NotifyListeners()
    {
        foreach (var listener in listeners)
        {
            listener(data);
        }
    }

    public void SetValue(string key, object value)
    {
        data[key] = value;
        NotifyListeners();
    }

    public T GetValue<T>(string key) => (T)data[key];

    public T? GetValueOrDefault<T>(string key) {
        if (data.TryGetValue(key, out object? value)) {
            return (T)value;
        }

        return default;
    }
    
    public void SetAll(IReadOnlyDictionary<string, object> newData)
    {
        data = newData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        NotifyListeners();
    }

    public class Source(ConfigurationBackgroundStore store) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(store);
    }

    public class Provider : ConfigurationProvider
    {
        private Dictionary<string, object> _rawData = new();

        public Provider(ConfigurationBackgroundStore store)
        {
            store.AddListener(data => {
                _rawData = data;
                Load();
            });
        }

        public override void Load()
        {
            Data = _rawData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString());
            OnReload();
        }
    }

    // In case we need multiple configuration stores
    private static Dictionary<string, ConfigurationBackgroundStore> Instances = new();
    public static ConfigurationBackgroundStore GetInstance(string name)
    {
        if (!Instances.ContainsKey(name))
        {
            Instances[name] = new ConfigurationBackgroundStore();
        }
        return Instances[name];
    }

    public class Factory 
    {
        public ConfigurationBackgroundStore GetStore(string name) => GetInstance(name);
    }
}

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
}
