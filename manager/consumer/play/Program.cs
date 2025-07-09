using Confi;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonHttp("http://localhost:40398/apps/thor/configuration");

builder.Configuration.AddFluentEnvironmentVariables();

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.Configure<ThorConfiguration>(builder.Configuration);

var app = builder.Build();

app.MapGet("/", (IOptionsSnapshot<ThorConfiguration> snapshot, IConfiguration configuration) =>
{
    return new
    {
        FromSnapshot = snapshot.Value,
        FromConfiguration = configuration["Nickname"]
    };
});

app.Run();

public record ThorConfiguration
{
    public required string Nickname { get; set; }
}

