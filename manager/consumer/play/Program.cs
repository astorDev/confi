using Confi;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

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

