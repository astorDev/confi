using Confi;
//using V1;
using V2;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

// v1:

// builder.Configuration.AddConfigurationStore(ConfigurationStore.Instance);
// builder.Configuration.AddConfigurationStore(ConfigurationStore.GetInstance(EnvironmentShuffling.Key));

// builder.Services.AddConfigurationStores();

// v2:

builder.Configuration.AddBackgroundStore();
builder.Configuration.AddBackgroundStore(EnvironmentShuffling.Key);

builder.Services.AddBackgroundConfigurationStores();

builder.Services.AddHostedService<Counting.BackgroundService>();
builder.Services.AddHostedService<EnvironmentShuffling.BackgroundService>();

var app = builder.Build();

app.MapGet("/counter", (IConfiguration config) => config[Counting.Key]);
app.MapGet("/env", (IConfiguration config) => config[EnvironmentShuffling.Key]);

app.Run();