namespace Confi.Json.Playground;

[TestClass]
public class JsonString
{
    [TestMethod]
    public void Basic()
    {
        var config = new ConfigurationBuilder()
            .AddJsonString("""
            {
                "greeting": "Hello from JSON String Config!",
                "nested": {
                    "message": "This is a nested message from JSON String Config!"
                }
            }
            """)
            .Build();

        var greeting = config["Greeting"];
        var nestedMessage = config["Nested:Message"];

        greeting.ShouldBe("Hello from JSON String Config!");
        nestedMessage.ShouldBe("This is a nested message from JSON String Config!");
    }

    [TestMethod]
    public void FinanceOption()
    {
        var config = new ConfigurationBuilder()
            .AddJsonString("""
            {
                "rates": {
                    "usd": 1.0,
                    "eur": 0.85,
                    "jpy": 110.0
                }
            }
            """)
            .Build();

        var option = config.Get<FinancialOption>();

        option.ShouldNotBeNull();
        option.Rates.ShouldNotBeNull();
        option.Rates.Count.ShouldBe(3);
        option.Rates["USD"].ShouldBe(1.0m);
        option.Rates["EUR"].ShouldBe(0.85m);
        option.Rates["JPY"].ShouldBe(110.0m);
    }
}

public class FinancialOption
{
    public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
}