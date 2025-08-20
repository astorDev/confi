using Confi;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfi("http://localhost:40398/thor", ConfiSchema.FromFile("thor.schema.json"));

builder.Configuration.AddFluentEnvironmentVariables();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

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