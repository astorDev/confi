namespace Confi.Manager.Tests;

[TestClass]
public class ClientTests
{
    [TestMethod]
    public async Task GetAboutPrefixed()
    {
        var http = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:40398/prefixed/")
        };

        var client = new Client(http, TestLogger.Of<Client>());

        await client.GetApp("playground");
    }
}