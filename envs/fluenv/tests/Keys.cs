namespace Confi.Tests;

[TestClass]
public class Keys
{
    [TestMethod]
    public void WebhookAddresses()
    {
        var keys = FluentEnvironmentVariablesConfiguration.Keys("WEBHOOK_ADDRESSES");
        foreach (var key in keys)
        {
            Console.WriteLine(key);
        }
    }

    [TestMethod]
    public void ThreeWordKey()
    {
        var keys = FluentEnvironmentVariablesConfiguration.Keys("THREE_WORD_KEY");
        foreach (var key in keys)
        {
            Console.WriteLine(key);
        }
    }
}