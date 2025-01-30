# Custom Configuration Provider in .NET: Step-by-Step Guide

> A Simple Example of an ASP .NET Core 9 Configuration Provider with Periodic Updates

ASP .NET Core apps come with an [impressive set](https://medium.com/@vosarat1995/net-configuration-architecture-getting-started-87526b9fbc68) of built-in configuration providers. Still, sometimes it's just not enough. Gladly, the .NET configuration system is versatile and allows us to implement our own custom configuration provider. So, how about we do just that?

## Wrapping Up!

We've implemented a custom configuration provider, that periodically updates the current count. We've also made the updates trackable by calling the `OnReload` method. Finally, we've verified the solution by using `IChangeToken` from `IConfiguration`. This should serve as a strong foundation for a custom configuration provider you may want to build.

You can find the source code for this article in the [Confi GitHub repository](https://github.com/astorDev/confi/blob/main/providers/dotnet/tests/CustomConfiguration.cs). Please, give the repository a star! ⭐

And also ... claps are appreciated! 👏
