# Configuration Provider in .NET Based on Background Service

> Connecting ASP .NET Core Configuration System with Dependency Injection enabled `IHostedService`

.NET provides a versatile configuration framework, allowing us to [implement a custom configuration provider](https://medium.com/@vosarat1995/custom-configuration-provider-in-net-step-by-step-guide-3d8a3a8f7203). Most of the times, the provider's goal is to read configuration data from some source in the background. In the meantime, ASP .NET Core provides us with a simple way to enable a DI-based background process by injecting an `IHostedService` into our DI container. However, there is no built-in way to connect those two. Gladly, there is a (slightly tricky) way to do it - let me show you!

> Or jump straight to the [TLDR](#tldr) for spoilers 😉 

## Create a Bridge Between DI-Container and Configuration 

```sh
dotnet new web
```

```csharp
public class ConfigurationStore
{
    public static ConfigurationStore Instance { get; } = new();
    private Dictionary<string, object> data = new();

    private ConfigurationStore(){}
}
```

```csharp
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
```

```csharp
public void SetValue(string key, object value)
{
    data[key] = value;
    NotifyListeners();
}

public void SetAll(IReadOnlyDictionary<string, object> newData)
{
    data = newData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    NotifyListeners();
}

public T GetValue<T>(string key) => (T)data[key];

public T? GetValueOrDefault<T>(string key) {
    if (data.TryGetValue(key, out object? value)) {
        return (T)value;
    }

    return default;
}
```

## Implementing our Background Configuration Provider

[article](https://medium.com/p/3d8a3a8f7203)

```csharp
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
```

## Assembling a Working System

[article](https://medium.com/p/d020c73b63a4)

```sh
dotnet add package Backi.Timers
```

```csharp
using Backi.Timers;

public class Counting
{
    public const string Key = "Counter";

    public class BackgroundService(ILogger<BackgroundService> logger, ConfigurationStore store) : IHostedService
    {
        private SafeTimer _timer = null!;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting counting timer");

            _timer = SafeTimer.RunNowAndPeriodically(
                TimeSpan.FromSeconds(1), 
                () => {
                    var currentValue = store.GetValueOrDefault<int>(Key);
                    logger.LogInformation("Incrementing counter. Current value: {currentValue}", currentValue);
                    store.SetValue(Key, currentValue + 1);
                },
                (ex) => logger.LogError(ex, "Error in counting timer")
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping counting timer");

            _timer.Stop();
            return Task.CompletedTask;
        }
    }
}
```

```csharp
builder.Configuration.AddConfigurationStore(ConfigurationStore.Instance);

builder.Services.AddSingleton(ConfigurationStore.Instance);

builder.Services.AddHostedService<Counting.BackgroundService>();
```

```csharp
app.MapGet("/counter", (IConfiguration config) => config.GetRequiredValue(Counting.Key));
```

![](logs-demo.png)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
```

```sh
curl localhost:5057/counter
```

`16`

## TLDR;

We've found a way to connect `IHostedService` with the Configuration system using the good old Singleton. Here's a complete code of the underlying store, allowing passing configuration data between services:

```csharp
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

    public void SetAll(IReadOnlyDictionary<string, object> newData)
    {
        data = newData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        NotifyListeners();
    }

    public T GetValue<T>(string key) => (T)data[key];

    public T? GetValueOrDefault<T>(string key) {
        if (data.TryGetValue(key, out object? value)) {
            return (T)value;
        }

        return default;
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
}
```

We've used the object in our `Counting.BackgroundService` to showcase the functionality. There are still some improvements to make, but I don't want to overflow this article, so maybe next time.

The source code for this article can be found [here on github](https://github.com/astorDev/confi/tree/main/providers/dotnet/background/playground). This article, as well as the underlying library, is part of the [project, called Confi](https://github.com/astorDev/confi) - don't hesitate to give it a star! ⭐

And also … claps are appreciated! 👏

