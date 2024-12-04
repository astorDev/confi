using dotenv.net;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.Logger.LogInformation("ConfigA: {ConfigA}", app.Configuration["ConfigA"]);

app.Run();

