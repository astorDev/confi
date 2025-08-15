- [ ] Self-Declaring Consumer. [Details](#self-declaring-consumer)
- [ ] UI Configuration Value Editing with Schema Validation

## Self-Declaring Consumer

```csharp
public static OptionsBuilder<ConfiSelfDeclarationSettings> AddConfi(this IApplicationBuilder builder, string connectionString)
{
    var connectionSettings, configSource = builder.Configuration.AddConfi(connectionString);

    // Registers IHostedService that logs by listening to events
    // Part of Confi.Json package. Or even some lower level package, used within Confi.Json
    builder.Services.AddConfigurationPollerLoggingBackgroundService(poller);

    // 1. Preconfigures ConfiSelfDeclarationSettings options (and returns builder for possible customizations)
    // 2. Registers Background Service, listeing to polling events and performing self-declaration based on the settings from the step 1
    return builder.Services.AddConfiSelfDeclarator(connectionSettings, poller);
}

public static (ConnectionSettings, JsonHttpConfiguration.Source) AddConfi(this ConfigurationManager configuration, string connectionString)
{
    connectionString ?= builder.Configuration["ConnectionStrings:Confi"] ?? builder.Configuration["Confi:ConnectionString"]
        ?? throw new ("""
            Confi Connection String is required. 
            but was neither passed directly nor available from 
            ConnectionStrings:Confi or Confi:ConnectionString configuration values
            """);

    var connectionSettings = ConnectionSettings.Parse(connectionString);

    var poller = builder.Configuration.AddJsonHttp($"{connectionSettings.BaseUrl}/apps/{connectionSettings.AppId}/configuration");
}
```