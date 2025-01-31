public class ConfigurationStore
{
    public static ConfigurationStore Instance { get; } = new();
    private Dictionary<string, object> data = new();

    private ConfigurationStore(){}

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

    public class Source(ConfigurationStore store) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(store);
    }

    public class Provider : ConfigurationProvider
    {
        private Dictionary<string, object> _rawData = new();

        public Provider(ConfigurationStore store)
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
    private static Dictionary<string, ConfigurationStore> Instances = new();
    public static ConfigurationStore GetInstance(string name)
    {
        if (!Instances.ContainsKey(name))
        {
            Instances[name] = new ConfigurationStore();
        }
        return Instances[name];
    }

    public class Factory 
    {
        public ConfigurationStore GetStore(string name) => GetInstance(name);
    }
}

public static class AppBuildingExtensions
{
    public static IConfigurationBuilder AddConfigurationStore(this IConfigurationBuilder configuration, ConfigurationStore store)
    {
        return configuration.Add(new ConfigurationStore.Source(store));
    }

    public static IServiceCollection AddConfigurationStores(this IServiceCollection services)
    {
        services.AddSingleton(ConfigurationStore.Instance);
        services.AddSingleton<ConfigurationStore.Factory>();
        return services;
    }
}