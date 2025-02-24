using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

public class Test
{
    public static readonly IMongoDatabase MongoDatabase = A.Fake<IMongoDatabase>();
    protected WebApplicationFactory Factory { get; } =  new();
    protected Client Client { get;  }

    protected Test()
    {
        var services = new ServiceCollection();
        services.AddLogging(l => l.AddSimpleConsole(c => c.SingleLine = true));
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<Confi.Manager.Client>>();
        
        Client = new(Factory.CreateClient(), logger);
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