using Microsoft.Extensions.Configuration;

namespace Confi.Tests;

[TestClass]
public class GetRequiredValueShould
{
    [TestMethod]
    [ExpectedException(typeof(MissingConfigurationKeyException))]
    public void ThrowExceptionWhenValueIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        configuration.GetRequiredValue("missingKey");
    }

    [TestMethod]
    public void ReturnConfigurationValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(){ { "existingKey", "existingValue" } })
            .Build();

        var value = configuration.GetRequiredValue("existingKey");
        Assert.AreEqual("existingValue", value);
    }
}