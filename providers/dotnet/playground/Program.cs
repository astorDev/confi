using Backi.Timers;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.AddBackgroundServiceConfiguration<Screaming.BackgroundService, Screaming.BackgroundService.Factory>();

builder.Configuration.AddCounting();

builder.Services.Configure<Counting.Model>(builder.Configuration);
builder.Services.Configure<Screaming.Model>(builder.Configuration);

builder.Services.AddHostedService<Counting.Watcher>();
builder.Services.AddHostedService<Screaming.Watcher>();

var app = builder.Build();

app.MapGet("/", (IOptions<Counting.Model> options) => options.Value);

app.MapGet("/snap", (IOptionsSnapshot<Counting.Model> options) => options.Value);

app.MapGet("/scream", (IOptions<Screaming.Model> options) => options.Value);

app.MapGet("scream/snap", (IOptionsSnapshot<Screaming.Model> options) => options.Value);

app.Run();


public static class Counting
{
    public const string Key = "Counter";

    public class Model 
    {
        public required string Counter { get; set; }
    }

    public static IConfigurationBuilder AddCounting(this IConfigurationBuilder configuration)
    {
        return configuration.Add(new Source());
    }

    public class Source : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new Provider();
        }
    }

    public class Provider : ConfigurationProvider
    {
        public Provider()
        {
            Data[nameof(Model.Counter)] = 1.ToString();

            SafeTimer.RunNowAndPeriodically(
                TimeSpan.FromSeconds(1),
                Load,
                ex => Console.WriteLine(ex.Message)
            );
        }

        public override void Load()
        {
            Data[nameof(Model.Counter)] = (Int32.Parse(Data[nameof(Model.Counter)]!) + 1).ToString();

            OnReload();
        }
    }

    public class Watcher(IOptionsMonitor<Counting.Model> monitor) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            monitor.OnChange(model => {
                Console.WriteLine("Detected counter change: " + model.Counter);
            });

            return Task.CompletedTask;
        }
    }
}