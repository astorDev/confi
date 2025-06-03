using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

public class Test
{
    public static readonly IMongoDatabase MongoDatabase = A.Fake<IMongoDatabase>();
    protected WebApplicationFactory Factory { get; } = new();
    protected Client Client { get; }

    protected Test()
    {
        Client = new(
            Factory.CreateClient(),
            TestLogger.Of<Client>()
        );
    }

    public class WebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureTestServices(s =>
            {
                s.AddSingleton(MongoDatabase);
            });
        }
    }
}

public class TestLogger
{
    private static IServiceProvider serviceProvider = buildServiceProvider();
    private static IServiceProvider buildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(l => l.AddSimpleConsole(c => c.SingleLine = true));
        return services.BuildServiceProvider();
    }

    public static ILogger<T> Of<T>()
    {
        return serviceProvider.GetRequiredService<ILogger<T>>();
    }
}