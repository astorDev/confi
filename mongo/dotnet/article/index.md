# MongoDB as .NET Configuration Provider

> Building Custom Configuration Provider to Read Configuration from MongoDB in an ASP .NET Core App.

An ASP .NET Core app, comes with multiple built-in Configuration Sources like `appsettings.json`, environment variables, and command line arguments. Unfortunately, neither of them provide a convenient way to update a configuration value on the fly. Things get especially complex when we are talking about a distributed system, when we need to update the settings just once and get them propogated to multiple application instances.

We need a database! MongoDB is JSON-based, so it comes with a dynamic schema and resemblence to appsettings.json - let's use it. All that's left to do is to build our MongoDB Configuration Provider.

## TLDR;

We've built a custom MongoDB configuration provider. The same provider could be found in a package, called `Confi.Mongo`. Here's how you can register one:

```csharp

```

After the registration, values of `simple` and `toggles` documents from the `configurations` collection will be automatically loaded to the  `IConfiguration`, which is all we need to do to integrate with the .NET configuration system.

The package is part of a [confi project](https://github.com/astorDev/confi), providing various configuration tools and best practices. Don't hesitate to give it a star! ⭐

And also... claps for this article are appreciated! 👏