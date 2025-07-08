var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => new
{
    Greeting = "Hello, from Confi.Json.Stub!",
    Nested = new
    {
        Message = "This is a nested message from confi.json.stub.",
        Number = Random.Shared.Next(1, 5)
    }
});

app.Run();
