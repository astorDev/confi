namespace Confi.Manager.Consumer.Playground;

[TestClass]
public class ConnectionString
{
    [TestMethod]
    public void ParseConnectionString()
    {
        var connectionString = "https://example.com/my-app";
        var settings = ConfiConnectionSettings.Parse(connectionString);
        settings.BaseUri.ShouldBe(new Uri("https://example.com"));
        settings.appId.ShouldBe("my-app");
    }

    [TestMethod]
    public void ParseWithQuery()
    {
        var connectionString = "https://example.com/q-app?query=param";
        var settings = ConfiConnectionSettings.Parse(connectionString);
        settings.BaseUri.ShouldBe(new Uri("https://example.com/"));
        settings.appId.ShouldBe("q-app");
    }

    [TestMethod]
    public void ThrowWithoutAppId()
    {
        var connectionString = "https://example.com/";
        Should.Throw<FormatException>(() => ConfiConnectionSettings.Parse(connectionString));
    }
}