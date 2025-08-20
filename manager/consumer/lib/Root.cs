using System.Text.Json;
using Confi.Manager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using JsonSchema = Confi.Manager.JsonSchema;

namespace Confi;

public static class ConfiRegistrator
{
    public static AsyncPoller<IDictionary<string, string?>>.ListenablesSet AddConfi(
        this IHostApplicationBuilder appBuilder,
        string? connectionString = null,
        JsonSchema? schema = null,
        string? version = null
    )
    {
        var listenables = appBuilder.Configuration.AddConfi(connectionString, schema, version);

        appBuilder.Services.AddJsonHttpLoggingBackgroundService(listenables);

        return listenables;
    }

    public static AsyncPoller<IDictionary<string, string?>>.ListenablesSet AddConfi(
        this IConfigurationManager configuration,
        string? connectionString = null,
        JsonSchema? schema = null,
        string? version = null
    )
    {
        var candidate = configuration.GetCandidate(schema);

        var connectionSettings = configuration.GetConnectionSettings(connectionString);
        version = configuration.GetVersion(version);

        return configuration.AddConfi(candidate, connectionSettings, version);
    }

    public static AsyncPoller<IDictionary<string, string?>>.ListenablesSet AddConfi(
        this IConfigurationBuilder configuration,
        AppVersionCandidate candidate,
        ConfiConnectionSettings connectionString,
        string version,
        TimeSpan? refreshInterval = null
    )
    {
        connectionString.PutCandidate(candidate, version).GetAwaiter().GetResult();
        return configuration.AddConfiPolling(connectionString, version, refreshInterval);
    }

    public static AsyncPoller<IDictionary<string, string?>>.ListenablesSet AddConfiPolling(
        this IConfigurationBuilder configuration,
        ConfiConnectionSettings connectionString,
        string version,
        TimeSpan? refreshInterval
    )
    {
        var pullUri = connectionString.PullUriString(version);
        return configuration.AddJsonHttp(pullUri, refreshInterval: refreshInterval ?? TimeSpan.FromSeconds(1));
    }
}

public static class ConfiSettingsResolver
{
    public static ConfiConnectionSettings GetConnectionSettings(this IConfiguration configuration, string? connectionString = null)
    {
        connectionString ??=
            configuration["Confi:ConnectionString"]
            ?? configuration.GetConnectionString("Confi")
            ?? throw new ArgumentException("Connection string for Confi is not provided and not found in configuration by `Confi:ConnectionString` or `ConnectionStrings:Confi` keys.");

        return ConfiConnectionSettings.Parse(connectionString);
    }

    public static string GetVersion(this IConfiguration configuration, string? version = null)
    {
        return version
               ?? configuration["Confi:Version"]
               ?? configuration["Version"]
               ?? Uris.Unversioned;
    }
}

public static class SelfDeclaration
{
    public static AppVersionCandidate GetCandidate(this IConfiguration configuration, JsonSchema? schema = null)
    {
        if (schema == null && !File.Exists("confi.schema.json")) throw new ArgumentException("Schema was not provided and confi.schema.json file does not exist in the current directory.");
        schema ??= ConfiSchema.FromFile("confi.schema.json");

        var initialConfig = configuration.RetrieveJson(schema).AsJsonElement();

        return new AppVersionCandidate(
            schema,
            initialConfig
        );
    }
}

public class ConfiSchema
{
    public static JsonSchema FromFile(string filePath)
    {
        var schemaString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<JsonSchema>(schemaString, JsonSerializerOptions.Web)!;
    }
}