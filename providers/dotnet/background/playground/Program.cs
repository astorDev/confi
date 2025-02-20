using Confi;
//using V1;
using V3;

// v0:
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Configuration.AddConfigurationStore(ConfigurationStore.Instance);

builder.Services.AddSingleton(ConfigurationStore.Instance);

builder.Services.AddHostedService<V1.Counting.BackgroundService>();

// v1:

// builder.Configuration.AddConfigurationStore(ConfigurationStore.Instance);
// builder.Configuration.AddConfigurationStore(ConfigurationStore.GetInstance(EnvironmentShuffling.Key));

// builder.Services.AddConfigurationStores();

// v2:

// builder.Configuration.AddBackgroundStore();
// builder.Configuration.AddBackgroundStore(EnvironmentShuffling.Key);

// builder.Services.AddBackgroundConfigurationStores();

// builder.Services.AddHostedService<Counting.BackgroundService>();
// builder.Services.AddHostedService<EnvironmentShuffling.BackgroundService>();

// v3:

// builder.AddBackgroundConfiguration<Counting.BackgroundService>(Counting.Key);
// builder.AddBackgroundConfiguration<EnvironmentShuffling.BackgroundService>(EnvironmentShuffling.Key);

var app = builder.Build();

app.MapGet("/counter", (IConfiguration config) => config.GetRequiredValue(Counting.Key));
app.MapGet("/env", (IConfiguration config) => config.GetRequiredValue(EnvironmentShuffling.Key));

app.Run();