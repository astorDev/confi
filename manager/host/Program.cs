using Astor.Logging;
using Scalar.AspNetCore;
using Fluenv;
using Nist.Logs;
using MongoDB.Driver;
using Persic;
using Confi;
using Nist;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddMiniJsonConsole();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Configuration.AddFluentEnvironmentVariables();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddMongo(
    _ => new MongoClient(builder.Configuration.GetRequiredValue("ConnectionStrings:Mongo")), 
    "confi-manager"
)
.AddConfiManagerCollections();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(s => s.WithTheme(ScalarTheme.DeepSpace));

app.UseHttpIOLogging(l => l.Message = HttpIOMessagesRegistry.DefaultWithJsonBodies);
app.UseProblemForExceptions(ex => 
    ex.ToConfiManagerError() ?? Errors.Unknown
);

app.MapGet($"/{Uris.About}", async (IHostEnvironment env, IMongoDatabase mongoDatabase) => new About(
    Description: "Confi.Manager",
    Version: typeof(Program).Assembly!.GetName().Version!.ToString(),
    Environment: env.EnvironmentName,
    Dependencies: new() {
        [ "mongo" ] = await mongoDatabase.Ping().ToJsonDocument()
    }
));

var prefix = app.Configuration["Prefix"];
_ = string.IsNullOrWhiteSpace(prefix)
    ? app.MapConfiManager()
    : app.MapGroup(prefix).MapConfiManager();

app.Run();

public partial class Program;