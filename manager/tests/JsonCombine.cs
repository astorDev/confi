using System.Text.Json;

namespace Confi.Manager.Tests;

[TestClass]
public class JsonCombine
{
    [TestMethod]
    public void Simple()
    {
        var latest = JsonSerializer.Deserialize<JsonElement>(
        """
        {
            "nickname" : "Tornado"
        }
        """);

        var next = JsonSerializer.Deserialize<JsonElement>(
        """
        {
            "nickname" : "Thor",
            "age" : 0.8
        }
        """);

        var rawSchema = """
        {
            "type": "object",
            "properties": {
                "nickname": { "type": "string" },
                "age": { "type": "number" }
            }
        }
        """;

        var schema = JsonSerializer.Deserialize<JsonSchema>(rawSchema, JsonSerializerOptions.Web)!;

        var result = schema.Combine(latest, next);

        result["nickname"]!.GetValue<string>().ShouldBe("Tornado");
        result["age"]!.GetValue<double>().ShouldBe(0.8);
    }

    [TestMethod]
    public void Nested()
    {
        var latest = JsonSerializer.Deserialize<JsonElement>(
        """
        {
            "nickname" : "Tornado",
            "address": {
                "city": "New York"
            }
        }
        """);

        var next = JsonSerializer.Deserialize<JsonElement>(
        """
        {
            "nickname" : "Thor",
            "age" : 0.8,
            "address": {
                "city": "Asgard",
                "country": "Norway"
            }
        }
        """);

        var rawSchema = """
        {
            "type": "object",
            "properties": {
                "nickname": { "type": "string" },
                "age": { "type": "number" },
                "address": {
                    "type": "object",
                    "properties": {
                        "city": { "type": "string" },
                        "country": { "type": "string" }
                    }
                }
            }
        }
        """;

        var schema = JsonSerializer.Deserialize<JsonSchema>(rawSchema, JsonSerializerOptions.Web)!;

        var result = schema.Combine(latest, next);

        result["nickname"]!.GetValue<string>().ShouldBe("Tornado");
        result["age"]!.GetValue<double>().ShouldBe(0.8);
        result["address"]!["city"]!.GetValue<string>().ShouldBe("New York");
        result["address"]!["country"]!.GetValue<string>().ShouldBe("Norway");
    }
}