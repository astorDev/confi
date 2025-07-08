using Confi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonString("""
{
    "greeting": "Hello from JSON String Config!",
    "nested": {
        "message": "This is a nested message from JSON String Config!"
    },
}
""");

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

var greeting = app.Configuration["Greeting"];
var nestedMessage = app.Configuration["Nested:Message"];

app.Logger.LogInformation("Greeting: {Greeting}", greeting);
app.Logger.LogInformation("Nested Message: {NestedMessage}", nestedMessage);

app.Run();