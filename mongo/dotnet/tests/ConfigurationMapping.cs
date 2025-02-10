using MongoDB.Bson;

namespace Confi.Mongo.Tests;

[TestClass]
public sealed class ConfigurationMapping
{
    [TestMethod]
    public void Versatile()
    {
        var sourceJson = """
        {
            "cool": true,
            "name" : "John",
            "logging": {
                "default": "warning",
                "providers" : [
                    {
                        "name": "Console",
                    },
                    {
                        "name": "File",
                        "path": "/var/log/app.log"
                    }
                ]
            }
        }
        """;

        var document = BsonDocument.Parse(sourceJson);

        var configs = document.ToConfigurationDictionary();

        foreach (var (key, value) in configs)
        {
            Console.WriteLine($"{key} => {value}");
        }
    }
}
