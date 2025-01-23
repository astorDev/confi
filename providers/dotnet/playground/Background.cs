using Backi.Timers;
using Microsoft.Extensions.Options;

public static class BackgroundServicesConfiguration
{
    private static readonly Dictionary<string, Store> stores = new();

    public static void AddBackgroundServiceConfiguration<THostedService, THostedServiceFactory>(this WebApplicationBuilder builder)
        where THostedService : class, IHostedService
        where THostedServiceFactory : class, IHostedServiceFactory<THostedService>
    {
        var hostedServiceType = typeof(THostedService).Name;  

        stores[hostedServiceType] = new Store();

        ((IConfigurationManager)builder.Configuration).Add(new Source(hostedServiceType));

        builder.Services.AddSingleton<IHostedServiceFactory<THostedService>, THostedServiceFactory>();
        builder.Services.AddHostedService(sp => {
            var factory = sp.GetRequiredService<IHostedServiceFactory<THostedService>>();
            return factory.Create(stores[hostedServiceType]);
        });
    }

    public interface IHostedServiceFactory<THostedService>
        where THostedService : class, IHostedService
    {
        THostedService Create(Store store);
    }

    public class Store
    {
        private Dictionary<string, object?> dictionary = new();
    
        private List<Action<Dictionary<string, object?>>> listeners = new();
    
        public void AddListener(Action<Dictionary<string, object?>> listener)
        {
            listeners.Add(listener);
            listener(dictionary);
        }

        public T GetRequiredValue<T>(string key) => (T)dictionary[key]!;
    
        public void SetValue(string key, object? value)
        {
            dictionary[key] = value;
            NotifyListeners();
        }

        public void NotifyListeners()
        {
            foreach (var listener in listeners)
            {
                listener(dictionary);
            }
        }
    }

    public class Source(string sourceName) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(sourceName);
    }

    public class Provider : ConfigurationProvider
    {
        private Dictionary<string, object?> _rawData = new();

        public Provider(string storeName)
        {
            var store = stores[storeName];

            store.AddListener(data => {
                Console.WriteLine("Provider notified");

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
}

public class Screaming
{
    public const string Key = "Scream";

    public class Model
    {
        public string? Scream { get; set; }
    }

    public class BackgroundService(
        BackgroundServicesConfiguration.Store store,
        ILogger<BackgroundService> logger) : IHostedService
    {
        private SafeTimer _timer = null!;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting screaming background service");

            store.SetValue(Key, "A");

            _timer = SafeTimer.RunNowAndPeriodically(
                TimeSpan.FromSeconds(5),
                () => {
                    var value = store.GetRequiredValue<string>(Key) + "A";
                    store.SetValue(Key, value);
                    logger.LogInformation("Screaming: {value}", value);
                },
                ex => logger.LogError(ex, "Error loading data")
            );
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping screaming background service");
            _timer.Stop();
        }

        public class Factory(ILogger<BackgroundService> logger) : BackgroundServicesConfiguration.IHostedServiceFactory<BackgroundService>
        {
            public BackgroundService Create(BackgroundServicesConfiguration.Store store)
            {
                return new BackgroundService(store, logger);
            }
        }
    }

    public class Watcher(IOptionsMonitor<Model> monitor) : IHostedService
    {
        public Task StartAsync(CancellationToken stoppingToken)
        {
            monitor.OnChange(model => {
                Console.WriteLine("Detected scream change: " + model.Scream);
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stopping scream watcher");

            return Task.CompletedTask;
        }
    }
}