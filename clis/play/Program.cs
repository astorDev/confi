using System.CommandLine;
using Confi;

var builder = WebApplication.CreateBuilder(args);

var readyFlag = new Option<bool?>("--ready", "-r");
var heroOption = new Option<string>("--hero");

var verboseFlag = new Option<bool?>("--verbose", "-v");
var quietFlag = new Option<bool?>("--quiet", "-q");
var logLevelOption = new Option<string>("--log-level");

builder.Configuration.AddCliOptions(args, 
    readyFlag.Configuring("Hero:Readiness", "on duty", "sleeping"),
    heroOption.Configuring("Hero:Name"),
    verboseFlag.Configuring("Logging:LogLevel:Default", "Trace", "Information"),
    quietFlag.Configuring("Logging:LogLevel:Default", "None", "Information"),
    logLevelOption.Configuring("Logging:LogLevel:Default")
);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.Logger.LogTrace("Reading hero configuration....");

var readiness = app.Configuration["Hero:Readiness"];
var heroName = app.Configuration["Hero:Name"];
var logLevelValue = app.Configuration.GetRequiredValue("Logging:LogLevel:Default");

app.Logger.LogInformation("{hero} is {Readiness}. Log level is {LogLevel}", heroName, readiness, logLevelValue);

app.MapGet("/", () => new {
    Message = "Hello World!"
});

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
await app.RunAsync(cts.Token);