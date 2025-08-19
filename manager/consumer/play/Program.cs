using System.Text.Json;
using System.Text.Json.Nodes;
using Confi;
using Confi.Manager;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:40398")
};

var schemaString = File.ReadAllText("confi.schema.json");
var schema = JsonSerializer.Deserialize<JsonSchema>(schemaString, JsonSerializerOptions.Web)!;
var initialConfigBuilder = new JsonObject
{
    ["nickname"] = builder.Configuration["nickname"]
};
var initialConfig = JsonSerializer.SerializeToElement(initialConfigBuilder, JsonSerializerOptions.Web);

var candidate = new AppVersionCandidate(
    schema,
    initialConfig
);

await httpClient.PutAsJsonAsync(Uris.AppUnversionedVersion("thor"), candidate);

var listenables = builder.Configuration.AddJsonHttp("http://localhost:40398/apps/thor/configuration");

builder.Configuration.AddFluentEnvironmentVariables();

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddJsonHttpLoggingBackgroundService(listenables);
builder.Services.Configure<ThorConfiguration>(builder.Configuration);

var app = builder.Build();

app.MapGet("/", (
    IOptionsSnapshot<ThorConfiguration> snapshot,
    IOptionsMonitor<ThorConfiguration> monitor,
    IOptions<ThorConfiguration> options,
    IConfiguration configuration) =>
{
    return new
    {
        FromSnapshot = snapshot.Value,
        FromMonitor = monitor.CurrentValue,
        FromOptions = options.Value,
        FromConfiguration = new {
            Nickname = configuration["Nickname"]
        }
    };
});

app.Run();

public record ThorConfiguration
{
    public required string Nickname { get; set; }
}