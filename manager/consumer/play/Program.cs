using Confi;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfi("http://localhost:40398/confi-play");

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.Run();