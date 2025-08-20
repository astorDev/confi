# Confi - Distributed Configuration Manager

Change your configuration in Confi - Watch it apply immediately across all app instances.

![](demo.gif)

Confi stores your configuration in mongo and provides simple HTTP endpoints for reads and updates. It prevents unauthorized changes using schema-driven approach. Confi consumer .NET library makes it easy to connect Confi as a configuration source to get on-the-fly updates. 

There's much more to it, but let's start by trying!

## Jump Start

In this section, we'll go through 4 steps you need to make to set and use Confi in an app:

### 1. Deploy a Local Instance of Confi

The most straightforward way to do is using docker compose. 

Here's an example `compose.yml`:

```yml
services:
  confi:
    image: vosarat/confi
    environment:
      - CONNECTION_STRINGS_MONGO=mongodb://confi-db:27017/
    ports:
      - "40398:8080"

  confi-db:
    image: mongo
    ports:
      - "27017:27017"
```

### 2. Set Your Configuration Schema Up.

Confi needs configuration schema, to know which parts of the configuration are allowed to be updated. We encourage to follow self-declaration strategy to achieve that, which means a consumer should supply it's own configuration schema to the Confi on the app start. For the jump start example, we'll declare a schema, allowing us to configure log level of the `Confi.JsonHttpConfiguration.Source`, which is used by a confi consumer to pull values from the manager:

To achieve that, place `confi.schema.json` file with the following content in the root of your project:

> Assumes you already have a .NET web project set up. Nothing fancy, even the simplest `dotnet new web` will do.

```json
{
    "$schema": "http://json-schema.org/draft-07/schema#",
    "type": "object",
    "properties": {
        "Logging": {
            "type": "object",
            "properties": {
                "LogLevel": {
                    "type": "object",
                    "properties": {
                        "Confi.JsonHttpConfiguration.Source" : {
                            "type": "string",
                            "enum": ["Information", "Debug", "Warning", "Error", "Critical"]
                        }
                    }
                }
            }
        }
    }
}
```

### 3. Add Confi as a Configuration Source

First, we'll need to install a consumer library:

```sh
dotnet add package Confi.Manager.Consumer
```

Then, in the  we'll just need to add confi as a configuration source:

> A good place is typically somewhere in the top of `Program.cs`:

```csharp
builder.AddConfi("http://localhost:40398/confi-play");
```

> 💡 `AddConfi` uses `IHostApplicationBuilder` and not just `IConfigurationManager` because it also attaches a background service that logs events, occuring during configuration polling. We'll update the way the logging behaves in the next step.

### 4. Update the Configuration and See It Effects.

Let's start by running our app, to make it declare itself and start using configuration values from Confi:

```sh
dotnet run
```

It should print the regular `Now listening ...` to console.

Now, let's update the configuration to use more verbose logging level for our configuration poller. Confi provides an HTTP API, so we can just PUT an updated configuration, using `curl`:

```sh
curl -X PUT http://localhost:40398/apps/confi-play/versions/unversioned/configuration \
-H "Content-Type: application/json" \
-d '{ "Logging": { "LogLevel": {
    "Confi.JsonHttpConfiguration.Source": "Debug"
  }}}'
```

After that our consumer app should start producing more logs to console:

![](demo.gif)

To finish the jump start, let's return logging level back to normal, as shown in the demo:

```sh
curl -X PUT http://localhost:40398/apps/confi-play/versions/unversioned/configuration \
-H "Content-Type: application/json" \
-d '{ "Logging": { "LogLevel": {
    "Confi.JsonHttpConfiguration.Source": "Information"
  }}}'
```

Congratulations! 
Your app now can be configured on the fly, from a centralized configuration manager, Confi.

---

Confi Manager is .NET [NIST](https://github.com/astorDev/nist) API.