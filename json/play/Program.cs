using Confi;

var builder = WebApplication.CreateBuilder(args);

var jsonHttpEvents = builder.Configuration.AddJsonHttp("http://localhost:5150/");

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

jsonHttpEvents.RegisterLogging(app.Services);

var greeting = app.Configuration["Greeting"];
var nestedMessage = app.Configuration["Nested:Message"];
var number = app.Configuration.GetValue<int>("Nested:Number");

app.Logger.LogInformation("Greeting: {Greeting}", greeting);
app.Logger.LogInformation("Nested Message: {NestedMessage}", nestedMessage);

app.MapGet("/", () => new
{
    Greeting = app.Configuration["Greeting"],
    Nested = new
    {
        Message = app.Configuration["Nested:Message"],
        Number = app.Configuration.GetValue<int>("Nested:Number")
    }
});

app.Run();