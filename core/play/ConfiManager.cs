namespace Confi.Core.Playground;

[TestClass]
public class ConfiManager
{
    public IServiceProvider BuildServices(Action<IConfigurationBuilder>? configure = null)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MyPath", "my schema" }
            });

        configure?.Invoke(configuration);

        services.AddSingleton<IConfiguration>(configuration.Build());

        return services.BuildServiceProvider();
    }

    [TestMethod]
    public void ResolveUsingConfiguration()
    {
        var serviceProvider = BuildServices();

        var builder = new ConfiConfigurationBuilder();
        var configuration = builder.Resolve(serviceProvider);

        configuration.Schema.ShouldBe("my schema");
    }

    [TestMethod]
    public void ResolveFromUpdatedPath()
    {
        var serviceProvider = BuildServices(configure: builder =>
        {
            builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UpdatedPath", "updated schema" }
            });
        });

        var builder = new ConfiConfigurationBuilder();

        builder.Schema.Path = "UpdatedPath";

        var configuration = builder.Resolve(serviceProvider);

        configuration.Schema.ShouldBe("updated schema");
    }

    [TestMethod]
    public void ResolveVersionWithoutProvider()
    {
        var serviceProvider = BuildServices();

        var builder = new ConfiConfigurationBuilder();
        var version = builder.Version.Resolve(serviceProvider);

        version.ShouldNotBeNull();
        version.ShouldBe("0.0.0");
    }
}

public class ConfiConfigurationBuilder
{
    public ConfigurationValue<string> Schema { get; set; } = ConfigurationValue<string>.ResolvedFromPath(
        "MyPath",
        (sp, path) => sp!.GetRequiredService<IConfiguration>().GetValue<string>(path!) ?? throw new InvalidOperationException("Schema path cannot be null.")
    );

    public ConfigurationValue<string> Version { get; set; } = new ConfigurationValue<string>(
        resolver: (sp, _) => sp.GetService<IVersionProvider>()?.Get() ?? "0.0.0"
    );

    public ConfiConfiguration Resolve(IServiceProvider sp)
    {
        return new ConfiConfiguration
        {
            Schema = Schema.Resolve(sp)
        };
    }
}

public class IVersionProvider
{
    public string Get() => "1.0.0";
}

public class ConfiConfiguration
{
    public required string Schema { get; set; }
}