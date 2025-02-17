using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Confi.BackgrounStore.Tests;

[TestClass]
public sealed class MultipleRegistrationsShould
{
    [TestMethod]
    public void WorkFine()
    {
        var configuration = new ConfigurationManager();
        var services = new ServiceCollection();

        configuration.AddBackgroundStore("Alpha");
        services.AddBackgroundConfigurationStores();

        configuration.AddBackgroundStore("Omega");
        services.AddBackgroundConfigurationStores();
        
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ConfigurationBackgroundStore.Factory>();
        
        var alphaStore = factory.GetStore("Alpha");
        var omegaStore = factory.GetStore("Omega");

        alphaStore.SetValue("Alpha", "bet");
        omegaStore.SetValue("Omega", 3);

        configuration.GetRequiredValue("Alpha").ShouldBe("bet");
        configuration.GetRequiredValue("Omega").ShouldBe("3");
    }
}
