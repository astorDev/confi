using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddFeatureManagement();

var app = builder.Build();

app.MapGet("/", async (IConfiguration configuration, IFeatureManager featureManager) => {
    return new {
        manager = await featureManager.IsEnabledAsync("One"),
        config = configuration["FeatureManagement:One"]
    };
});

app.Run();

public class ExampleConfiguration
{
    public string Key { get; set; } = null!;
}