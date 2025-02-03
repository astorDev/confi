# Custom Configuration Provider in .NET: Step-by-Step Guide

> A Simple Example of an ASP .NET Core 9 Configuration Provider with Periodic Updates

ASP .NET Core apps come with an [impressive set](https://medium.com/@vosarat1995/net-configuration-architecture-getting-started-87526b9fbc68) of built-in configuration providers. Still, sometimes it's just not enough. Gladly, the .NET configuration system is versatile and allows us to implement our own custom configuration provider. So, how about we do just that?

## Creating the Simplest Provider Possible

First thing first, we need to make sure we have all the required packages installed. In a Web template, the package we need is already installed, but in the case of a library, test, or console application we'll need to run the following:

> This assumes you are in a folder containing a .NET project. The easiest way to create one is by running `dotnet new console`.

```sh
dotnet add package Microsoft.Extensions.Configuration
```

We'll start with a very basic example - a provider incrementing the `Counter` configuration value. We'll need the configuration value key:

```cs
public const string Key = "Counter";
```

Then, we'll be able to let our provider just set it to `1`:

```cs
public class Provider : ConfigurationProvider {
    public override void Load() => Data[Key] = 1.ToString();
}
```

Finally, to be able to inject our provider into the configuration system we'll need an `IConfigurationSource`, that will create the provider:

```cs
public class Source : IConfigurationSource {
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider();
}
```

Combining it all together we should get something like this:

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

Let's now use our provider and see what we'll get after accessing our `Counting` configuration value. Here's how we can do it:

```cs
var config = new ConfigurationBuilder()
    .Add(new Counting.Source())
    .Build();

Console.WriteLine($"Current Count: {config[Counting.Key]}");
```

This is what we'll get after running the code:

```text
Current Count: 1
```

This is how we can create a very basic custom configuration provider in .NET. But the system allows us to make something far more interesting - let's try what else we can do.

## Implementing Periodic Configuration Updates

As of now, our `Counter` doesn't really count anything. To fix that, we'll need a background timer triggering the update. Let's install a helper package for that:

> I've written a dedicated [article about timers in .NET](https://medium.com/@vosarat1995/net-timers-all-you-need-to-know-d020c73b63a4), that investigates various ways we can use .NET timers. The article also explains how exactly the package below works. You can check it out, but the code should be pretty self-explanatory

```sh
dotnet add package Backi.Timers
```

Now, let's define a proper logic of assigning the timer, which we'll assign `0` the first time the loading is triggered and increment the existing value by `1` if our key is already present:

```cs
public override void Load() {
    if (!Data.TryGetValue(Key, out var currencyValueRaw)) {
        currencyValueRaw = "0";
    }

    Data[Key] = (Int32.Parse(currencyValueRaw!) + 1).ToString();
}
```

We'll need to set a timer running every one-tenth of a second, executing the `Load` method, and writing a possibly occurred exception to the console:

```cs
SafeTimer.RunNowAndPeriodically(
    TimeSpan.FromSeconds(0.1),
    Load,
    ex => Console.WriteLine(ex.Message)
);
```

We'll start the timer from our provider constructor. Here's what our provider should look like after the update:

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

If we'll run the code right now we will get the same result as before.

```text
Current Count: 1
```

Let's update the executing code by asking for the current configuration value 150 milliseconds after the first update:

```cs
Console.WriteLine($"Current Count: {config[Counting.Key]}");
await Task.Delay(150);
Console.WriteLine($"Current Count: {config[Counting.Key]}");
```

Now we should get an incrementing value in the second row we print.

```text
Current Count: 1
Current Count: 3
```

We have implemented a proper `Counting` configuration provider, that updates its value once in a while. Still, there's one thing we need to do in order to make our `ConfigurationProvider` feature complete. Let's do it in the next section!

## Notifying & Listening Configuration Changes

One of the coolest parts about the .NET Configuration system is its ability to work with dynamic configuration. Configuration sources may update their value on the go and the system allows us to know about such updates.

Here's how we can implement the listening part:

```cs
config.GetReloadToken().RegisterChangeCallback(_ => {
    Console.WriteLine($"Changed... Current Count: {config[Counting.Key]}");
}, null);
```

Let's also increment the wait between reading the configuration to see more about what's going on:

```cs
await Task.Delay(400);
```

Here's the complete code we'll get:

```cs
var config = new ConfigurationBuilder()
    .Add(new Counting.Source())
    .Build();

config.GetReloadToken().RegisterChangeCallback(_ => {
    Console.WriteLine($"Changed... Current Count: {config[Counting.Key]}");
}, null);

Console.WriteLine($"Current Count: {config[Counting.Key]}");
await Task.Delay(400);
Console.WriteLine($"Current Count: {config[Counting.Key]}");
```

Let's run the code! Here's what we should get:

```text
Current Count: 1
Current Count: 5
```

As you may see, there's no `Changed...` message printed. That's because we have notified the system about the updates. Gladly, that's very easy to do - we'll just need to call the `OnReload` method we inherited from the `ConfigurationProvider`.

We update our configuration value every time the `Load` method is called, therefore after updating the value, we should notify about the fact that changes have happened. Here's how our updated `Load` method should look like:

```cs
public override void Load() {
    if (!Data.TryGetValue(Key, out var currencyValueRaw)) {
        currencyValueRaw = "0";
    }
                
    Data[Key] = (Int32.Parse(currencyValueRaw!) + 1).ToString();

    OnReload();
}
```

Now, if we run the same code as before we'll get the following.

```text
Current Count: 2
Changed... Current Count: 3
Current Count: 5
```

Surprisingly, the `Changed...` message was printed only once. But there's nothing wrong with our provider. This is happening due to the way the configuration reload token works - after an update, the token seems to be "used" and doesn't notify about the following changes.

To do a proper subscription we'll need to get a new reload token and register a new callback every time the current callback has happened. We can do it via recursion like that:

```cs
private void Watch(IConfiguration config) {
    _ = config.GetReloadToken().RegisterChangeCallback(_ => {
        Console.WriteLine($"Changed... Current Count: {config[Counting.Key]}");
        Watch(config);
    }, null);
}
```

Finally, we should use the method and get the following code:

```cs
var config = new ConfigurationBuilder()
    .Add(new Counting.Source())
    .Build();

Watch(config);

Console.WriteLine($"Current Count: {config[Counting.Key]}");
await Task.Delay(400);
Console.WriteLine($"Current Count: {config[Counting.Key]}");
```

After running it we should see every update printed to the console:

```text
Current Count: 2
Changed... Current Count: 3
Changed... Current Count: 4
Changed... Current Count: 5
Changed... Current Count: 6
Current Count: 6
```

Now, we have a proper counter with updates listening! There's nothing more left in terms of configuration provider fundamentals, so let's wrap it up!

## Wrapping Up!

We've implemented a custom configuration provider in .NET, that periodically updates the current count. We've also made the updates trackable by calling the `OnReload` method. Finally, we've verified the solution by using `IChangeToken` from `IConfiguration`. This should serve as a strong foundation for a custom configuration provider you may want to build.

You can find the source code for this article in the [Confi GitHub repository](https://github.com/astorDev/confi/blob/main/providers/dotnet/tests/CustomConfiguration.cs). Please, give the repository a star! ⭐

And also ... claps are appreciated! 👏
