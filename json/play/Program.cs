using Confi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonHttp("https://gist.githubusercontent.com/astorDev/17ea7a787893c5af0ebe9cacdecc3391/raw/5b7d17cac5572f3d2c4fbccf45f0a72ff66a4cd6/config.json");

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

await Task.Delay(1000); // Wait for the configuration to load

var greeting = app.Configuration["Greeting"];
var nestedMessage = app.Configuration["Nested:Message"];

app.Logger.LogInformation("Greeting: {Greeting}", greeting);
app.Logger.LogInformation("Nested Message: {NestedMessage}", nestedMessage);

app.Run();