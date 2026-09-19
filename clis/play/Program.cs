using System.CommandLine;
using Confi;

var builder = WebApplication.CreateBuilder(args);

var ready = new Option<bool>("--ready", "-r");
var hero = new Option<string>("--hero");

var verbose = new Option<bool>("--verbose", "-v");
var quiet = new Option<bool>("--quiet", "-q");
var logLevel = new Option<string>("--log-level");

builder.Configuration.AddCli(args, 
    CliConfig.Flag(ready, "Hero:Readiness", "on duty", "sleeping"),
    CliConfig.Mirrored(hero, "Hero:Name"),
    CliConfig.Flag(verbose, "Logging:LogLevel:Default", "Trace", "Information"),
    CliConfig.Flag(quiet, "Logging:LogLevel:Default", "None", "Information"),
    CliConfig.Mirrored(logLevel, "Logging:LogLevel:Default")
);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.Logger.LogTrace("Reading hero configuration....");

var readiness = app.Configuration.GetRequiredValue("Hero:Readiness");
var heroName = app.Configuration.GetRequiredValue("Hero:Name");

app.Logger.LogInformation("{hero} is {Readiness}", heroName, readiness);

app.MapGet("/", () => new {
    Message = "Hello World!"
});

app.Run();