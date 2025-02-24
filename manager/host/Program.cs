using Astor.Logging;
using Scalar.AspNetCore;
using Fluenv;
using Nist.Logs;
using Nist.Errors;
using MongoDB.Driver;
using Persic;
using Confi;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddMiniJsonConsole();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Configuration.AddFluentEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddMongo(
    sp => new MongoClient(builder.Configuration.GetRequiredValue("ConnectionStrings:Mongo")), 
    "confi-manager"
);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(s => s.WithTheme(ScalarTheme.DeepSpace));

app.UseHttpIOLogging(l => l.Message = HttpIOMessagesRegistry.DefaultWithJsonBodies);
app.UseErrorBody(ex => ex switch {
    _ => Errors.Unknown
});

app.MapGet($"/{Uris.About}", async (IHostEnvironment env, IMongoDatabase mongoDatabase) => new About(
    Description: "Confi.Manager",
    Version: typeof(Program).Assembly!.GetName().Version!.ToString(),
    Environment: env.EnvironmentName,
    Dependencies: new Dictionary<string, object> {
        [ "mongo" ] = await mongoDatabase.Ping().ToJsonDocument()
    }
));

app.Run();

public partial class Program;