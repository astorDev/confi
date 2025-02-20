## TLDR;

We've found a way to connect `IHostedService` with the Configuration system using the good old Singleton. We've also made a small system around it to make it seamless and versatile. To not make this on your own you can just use the following package:

```sh
dotnet add package Confi.BackgroundStore
```

You then will need to `AddBackgroundStore` as the configuration source, where you can use one keyless store and as many keyed stores as you want. As a second step, you'll need to add those stores to the DI container and finally use those stores in their dedicated `IHostedService`. Just like that:

> Of course, you can always read the article again or ask in the comments if something is unclear 😉

```csharp
builder.Configuration.AddBackgroundStore();
builder.Configuration.AddBackgroundStore(EnvironmentShuffling.Key);

builder.Services.AddBackgroundConfigurationStores();

builder.Services.AddHostedService<Counting.BackgroundService>();
builder.Services.AddHostedService<EnvironmentShuffling.BackgroundService>();
```

The source code for this article can be found [here on github](https://github.com/astorDev/confi/tree/main/providers/dotnet/background/playground). This article, as well as the underlying library, is part of the [project, called Confi](https://github.com/astorDev/confi) - don't hesitate to give it a star! ⭐

And also … claps are appreciated! 👏