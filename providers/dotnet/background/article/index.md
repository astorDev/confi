# Configuration Provider in .NET Based on Background Service

> Connecting ASP .NET Core Configuration System with Dependency Injection enabled `IHostedService`

.NET provides a versatile configuration framework, allowing us to [implement a custom configuration provider](https://medium.com/@vosarat1995/custom-configuration-provider-in-net-step-by-step-guide-3d8a3a8f7203). Most of the times, the provider's goal is to read configuration data from some source in the background. In the meantime, in ASP .NET Core provides us with a simple way to enable a DI-based background process by injecting an `IHostedService` to our DI-container. However, there is no built-in way to connect those two. Gladly, there is a (slightly tricky) way to do it - let me show it to you!

> Or jump straight to the [TLDR](#tldr) for spoilers 😉 

## TLDR;

We've found a way to connect `IHostedService` with Configuration system by using the good old Singleton. We've also made a small system around it to make it seemless and versatile. To not make this on your own you can just use the following package:

```sh
dotnet add package Confi.BackgroundStore
```

You then will need to `AddBackgroundStore` as the configuration sources, where you can use one keyless store and as many keyed as stores as you want. As a second step, you'll need to add those stores to DI-container and finally use those stores in their dedicated `IHostedService`s. Just like that:

> Of course, you can always read the article again or ask in the comments if something is unclear 😉

```csharp
builder.Configuration.AddBackgroundStore();
builder.Configuration.AddBackgroundStore(EnvironmentShuffling.Key);

builder.Services.AddBackgroundConfigurationStores();

builder.Services.AddHostedService<Counting.BackgroundService>();
builder.Services.AddHostedService<EnvironmentShuffling.BackgroundService>();
```

Source code for this article could be found [here on github](https://github.com/astorDev/confi/tree/main/providers/dotnet/background/playground). This article, as well as the underlying library, is part of the [project, called Confi](https://github.com/astorDev/confi) - don't hesitate to give it a star! ⭐

And also … claps are appreciated! 👏

