using Backi.Timers;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Confi.Providers.Tests;

[TestClass]
public sealed class CustomConfiguration
{
    [TestMethod]
    public void Basic()
    {
        var config = new ConfigurationBuilder()
            .Add(new Counting.Basic.Source())
            .Build();

        Console.WriteLine($"Current Count: {config[Counting.Key]}");
    }

    [TestMethod]
    public void IncrementingAtTheStart()
    {
        var config = new ConfigurationBuilder()
            .Add(new Counting.Timered.Source())
            .Build();

        Console.WriteLine($"Current Count: {config[Counting.Key]}");
    }

    [TestMethod]
    public async Task Incrementing()
    {
        var config = new ConfigurationBuilder()
            .Add(new Counting.Timered.Source())
            .Build();

        Console.WriteLine($"Current Count: {config[Counting.Key]}");
        await Task.Delay(150);
        Console.WriteLine($"Current Count: {config[Counting.Key]}");

        config[Counting.Key].ShouldBe("3");
    }

    [TestMethod]
    public async Task ListeningUnfinal()
    {
        var config = new ConfigurationBuilder()
            .Add(new Counting.Timered.Source())
            .Build();

        config.GetReloadToken().RegisterChangeCallback(_ => {
            Console.WriteLine($"Changed... Current Count: {config[Counting.Key]}");
        }, null);

        Console.WriteLine($"Current Count: {config[Counting.Key]}");
        await Task.Delay(400);
        Console.WriteLine($"Current Count: {config[Counting.Key]}");
    }

    [TestMethod]
    public async Task ListeningFinal()
    {
        var config = new ConfigurationBuilder()
            .Add(new Counting.Final.Source())
            .Build();

        config.GetReloadToken().RegisterChangeCallback(_ => {
            Console.WriteLine($"Changed... Current Count: {config[Counting.Key]}");
        }, null);

        Console.WriteLine($"Current Count: {config[Counting.Key]}");
        await Task.Delay(400);
        Console.WriteLine($"Current Count: {config[Counting.Key]}");
    }

    [TestMethod]
    public async Task ListeningFinalProperly()
    {
        var config = new ConfigurationBuilder()
            .Add(new Counting.Final.Source())
            .Build();

        Watch(config);

        Console.WriteLine($"Current Count: {config[Counting.Key]}");
        await Task.Delay(400);
        Console.WriteLine($"Current Count: {config[Counting.Key]}");
    }

    private void Watch(IConfiguration config) {
        _ = config.GetReloadToken().RegisterChangeCallback(_ => {
            Console.WriteLine($"Changed... Current Count: {config[Counting.Key]}");
            Watch(config);
        }, null);
    }
}

public static class Counting
{
    public const string Key = "Counter";

    public class Basic
    {
        public class Source : IConfigurationSource {
            public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider();
        }

        public class Provider : ConfigurationProvider {
            public override void Load() => Data[Key] = 1.ToString();
        }
    }

    public class Timered
    {
        public class Source : IConfigurationSource {
            public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider();
        }

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
    }

    public class Final
    {
        public class Source : IConfigurationSource {
            public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider();
        }

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

                OnReload();
            }
        }
    }

    public static IConfigurationBuilder AddCounting(this IConfigurationBuilder configuration)
    {
        return configuration.Add(new Source());
    }

    public class Source : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new Provider();
        }
    }

    public class Provider : ConfigurationProvider
    {
        public Provider()
        {
            Data[Key] = 1.ToString();

            SafeTimer.RunNowAndPeriodically(
                TimeSpan.FromSeconds(1),
                Load,
                ex => Console.WriteLine(ex.Message)
            );
        }

        public override void Load()
        {
            var currentValue = Int32.Parse(Data[Key]!);
            Data[Key] = (currentValue + 1).ToString();

            //OnReload();
        }
    }
}