var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Configuration.AddConfigurationStore(ConfigurationStore.Instance);
builder.Configuration.AddConfigurationStore(ConfigurationStore.GetInstance(EnvironmentShuffling.Key));

builder.Services.AddConfigurationStores();

builder.Services.AddHostedService<Counting.BackgroundService>();
builder.Services.AddHostedService<EnvironmentShuffling.BackgroundService>();

var app = builder.Build();

app.MapGet("/counter", (IConfiguration config) => config[Counting.Key]);
app.MapGet("/env", (IConfiguration config) => config[EnvironmentShuffling.Key]);

app.Run();