# Custom Configuration Provider in .NET: Step-by-Step Guide

> A Simple Example of an ASP .NET Core 9 Configuration Provider with Periodic Updates

ASP .NET Core apps come with an [impressive set](https://medium.com/@vosarat1995/net-configuration-architecture-getting-started-87526b9fbc68) of built-in configuration providers. Still, sometimes it's just not enough. Gladly, the .NET configuration system is versatile and allows us to implement our own custom configuration provider. So, how about we do just that?

## Creating the Simplest Provider Possible

> This assumes you are in a folder containing a .NET project. The easiest way to create one is by running `dotnet new console`.

```sh
dotnet add package Microsoft.Extensions.Configuration
```

```cs
public const string Key = "Counter";
```

```cs
public class Provider : ConfigurationProvider {
    public override void Load() => Data[Key] = 1.ToString();
}
```

```cs
public class Source : IConfigurationSource {
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider();
}
```

```cs
public static class Counting
{
    public const string Key = "Counter";

    public class Source : IConfigurationSource {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider();
    }

    public class Provider : ConfigurationProvider {
        public override void Load() => Data[Key] = 1.ToString();
    }
}
```

```cs
var config = new ConfigurationBuilder()
    .Add(new Counting.Source())
    .Build();

Console.WriteLine($"Current Count: {config[Counting.Key]}");
```

```text
Current Count: 1
```

## Implementing Periodic Configuration Updates

```sh
dotnet add package Backi.Timers
```

```cs
if (!Data.TryGetValue(Key, out var currencyValueRaw)) {
    currencyValueRaw = "0";
}
    
Data[Key] = (Int32.Parse(currencyValueRaw!) + 1).ToString();
```

```cs
SafeTimer.RunNowAndPeriodically(
    TimeSpan.FromSeconds(0.1),
    Load,
    ex => Console.WriteLine(ex.Message)
);
```

```cs
public class Provider : ConfigurationProvider {
    public Provider() => SafeTimer.RunNowAndPeriodically(
        TimeSpan.FromSeconds(0.1),
        Load,
        ex => Console.WriteLine(ex.Message)
    );

    public override void Load() {
        if (!Data.TryGetValue(Key, out var currencyValueRaw)) {
            currencyValueRaw = "0";
        }
        
        Data[Key] = (Int32.Parse(currencyValueRaw!) + 1).ToString();
    }
}
```

```text
Current Count: 1
```

```cs
Console.WriteLine($"Current Count: {config[Counting.Key]}");
await Task.Delay(150);
Console.WriteLine($"Current Count: {config[Counting.Key]}");
```

```text
Current Count: 1
Current Count: 3
```

## Notifying & Listening Configuration Changes

```cs
config.GetReloadToken().RegisterChangeCallback(_ => {
    Console.WriteLine($"Changed... Current Count: {config[Counting.Key]}");
}, null);
```

```text
Current Count: 1
Current Count: 3
```

```cs
OnReload();
```

```cs
await Task.Delay(400);
```

```text
Current Count: 2
Changed... Current Count: 3
Current Count: 5
```

```cs
private void Watch(IConfiguration config) {
    _ = config.GetReloadToken().RegisterChangeCallback(_ => {
        Console.WriteLine($"Changed... Current Count: {config[Counting.Key]}");
        Watch(config);
    }, null);
}
```

```cs
var config = new ConfigurationBuilder()
    .Add(new Counting.Final.Source())
    .Build();

Watch(config);

Console.WriteLine($"Current Count: {config[Counting.Key]}");
await Task.Delay(400);
Console.WriteLine($"Current Count: {config[Counting.Key]}");
```

```text
 Current Count: 2
 Changed... Current Count: 3
 Changed... Current Count: 4
 Changed... Current Count: 5
 Changed... Current Count: 6
 Current Count: 6
```

## Wrapping Up!

We've implemented a custom configuration provider, that periodically updates the current count. We've also made the updates trackable by calling the `OnReload` method. Finally, we've verified the solution by using `IChangeToken` from `IConfiguration`. This should serve as a strong foundation for a custom configuration provider you may want to build.

You can find the source code for this article in the [Confi GitHub repository](https://github.com/astorDev/confi/blob/main/providers/dotnet/tests/CustomConfiguration.cs). Please, give the repository a star! ⭐

And also ... claps are appreciated! 👏
