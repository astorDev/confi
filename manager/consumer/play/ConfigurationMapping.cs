namespace Confi.Manager.Consumer.Playground;

[TestClass]
public class ConfigurationMapping
{
    [TestMethod]
    public void Basic()
    {
        var schema = """
        {
            "type": "object",
            "properties": {
                "message": {
                    "type": "string"
                }
            }
        }
        """;

        var configuration = new Dictionary<string, string?>
        {
            { "message", "Hello, World!" },
            { "anotherMessage", "This should not be included" }
        };

        Print(configuration, schema);
    }

    [TestMethod]
    public void Nested()
    {
        var schema = """
        {
            "type": "object",
            "properties": {
                "message": {
                    "type": "string"
                },
                "nested": {
                    "type": "object",
                    "properties": {
                        "innerMessage": {
                            "type": "string"
                        }
                    }
                }
            }
        }
        """;

        var configuration = new Dictionary<string, string?>
        {
            { "message", "Hello, World!" },
            { "nested:innerMessage", "Nested Hello!" },
            { "anotherMessage", "This should not be included" }
        };

        Print(configuration, schema);
    }

    [TestMethod]
    public void AdditionalProperties()
    {
        var schema = """
        {
            "type": "object",
            "additionalProperties": {
                "type": "object",
                "properties": {
                    "Name": {
                        "type": "string"
                    }
                },
                "required": [
                    "Name"
                ]
            }
        }
        """;

        var configuration = new Dictionary<string, string?>
        {
            { "USD:Name", "US Dollar" },
            { "EUR:Name", "Euro" },
            { "GBP:Name", "British Pound" }
        };

        Print(configuration, schema);
    }

    [TestMethod]
    public void NestAdditionalProperties()
    {
        var schema = """
        {
            "type": "object",
            "properties": {
                "Currencies": {
                    "type": "object",
                    "additionalProperties": {
                        "type": "object",
                        "properties": {
                            "Name": {
                                "type": "string"
                            }
                        },
                        "required": [
                            "Name"
                        ]
                    }
                }
            }
        }
        """;

        var configuration = new Dictionary<string, string?>
        {
            { "Currencies:USD:Name", "US Dollar" },
            { "Currencies:EUR:Name", "Euro" },
            { "Currencies:GBP:Name", "British Pound" }
        };


        Print(configuration, schema);
    }

    public static void Print(Dictionary<string, string?> configuration, string schema)
    {
        var configurationObject = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .Build();

        var schemaNode = JsonSchema.Parse(schema);

        var configurationJson = configurationObject.AsJson(schemaNode);
        Console.WriteLine(configurationJson);
    }
}