## .env in .NET

> Connecting the Dots

Sometimes, a development team may want to have a configuration variable individual for every developer. There's no better tool to achieve that than `.env` files. However, .NET applications do not include this configuration source by default. Gladly, that's easy to fix. Let's do just that!

## 

```sh
dotnet new web --name DotEnvs.Playground
```

`cd DotEnvs.Playground`


```sh
dotnet publish && cd bin/Release/net9.0/publish && dotnet DotEnvs.Playground.dll && cd ../../../..
```

> Unlike `CopyToOutputDirectory` `CopyToPublishDirectory` does **not** make the file required.

## TLDR;

`.env` files are one of the most fundamental sources of configuration. .NET doesn't include it by default, sill using the `dotenv.net` nuget package we can easily add the source to our app. Just make sure not to forget:

1. Load environment variables from the file:

```csharp
using dotenv.net;

DotEnv.Load();
``` 

2. Include the file in the publish directory. (in case it's not only used for local debugging)

```xml
<Content Include=".env" CopyToPublishDirectory="Always"/>
```

3. Add `.env` to `.gitignore`.

Since the main use-case of this file is to be developer-specific.

4. Clap for this article 👏

Well, that's not required... but would be appreciated 👉👈
