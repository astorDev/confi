namespace Confi.Manager.Consumer.Playground;

[TestClass]
public class Retrieval
{
    [TestMethod]
    public void Simple()
    {
        var schema = """
        {
            "type": "object",
            "properties": {
                "nickname": {
                    "type": "string"
                }
            }
        }
        """.ParseAsJsonSchema();

        var configuration = new ConfigurationManager();

        configuration["nickname"] = "Thor";
        configuration["anotherProperty"] = "This should not be included";

        var result = configuration.RetrieveJson(schema).AsJsonElement();

        Console.WriteLine(result.GetRawText());

        result.TryGetProperty("nickname", out var nicknameProperty).ShouldBeTrue();
        nicknameProperty.GetString().ShouldBe("Thor");
        result.TryGetProperty("anotherProperty", out _).ShouldBeFalse();
    }

    [TestMethod]
    public void Nested()
    {
        var schema = """
        {
            "type": "object",
            "properties": {
                "nickname": {
                    "type": "string"
                },
                "master" : {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string"
                        },
                        "age": {
                            "type": "integer"
                        }
                    }
                }
            }
        }
        """.ParseAsJsonSchema();

        var configuration = new ConfigurationManager();

        configuration["nickname"] = "Thor";
        configuration["anotherProperty"] = "This should not be included";
        configuration["master:name"] = "Odin";
        configuration["master:age"] = "100";

        var result = configuration.RetrieveJson(schema).AsJsonElement();

        Console.WriteLine(result.GetRawText());

        result.TryGetProperty("nickname", out var nicknameProperty).ShouldBeTrue();
        nicknameProperty.GetString().ShouldBe("Thor");
        result.TryGetProperty("master", out var masterProperty).ShouldBeTrue();
        masterProperty.TryGetProperty("name", out var nameProperty).ShouldBeTrue();
        nameProperty.GetString().ShouldBe("Odin");
        masterProperty.TryGetProperty("age", out var ageProperty).ShouldBeTrue();
        ageProperty.GetInt32().ShouldBe(100);
    }
}