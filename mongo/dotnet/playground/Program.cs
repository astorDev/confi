using System.Text.Json;
using Confi;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddBackgroundStore(MongoConfigurationLoader.Key);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddMongo("mongodb://localhost:27017/?replicaSet=rs0", "confi-playground")
    .AddCollection<ConfigRecord>("configurations");

builder.Services.AddBackgroundConfigurationStores();
builder.Services.AddMongoBackgroundConfigurationLoader("simple");
builder.Services.AddMongoBackgroundConfigurationLoader("toggles");

var app = builder.Build();

app.MapPut("simple/config", async (IMongoCollection<ConfigRecord> collection, IConfiguration configuration, JsonElement body) => {
    var result = await collection.Put(body.ToDbRecord("simple"));

    await Task.Delay(100);
    
    return new {
        sent = body,
        config = new {
            name = configuration["name"],
            age = configuration["age"],
        }
    };
});

app.MapPut("toggles/config", async (IMongoCollection<ConfigRecord> collection, IConfiguration configuration, JsonElement body) => {
    var result = await collection.Put(body.ToDbRecord("toggles"));

    await Task.Delay(100);
    
    return new {
        sent = body,
        config = new {
            featureManagement = new {
                featureA = configuration["featureManagement:featureA"],
                featureB = new {
                    percentage = configuration["featureManagement:featureB:percentage"],
                }
            }
        }
    };
});

app.Run();

public record AppConfiguration(string Id, JsonElement Value)
{
    public static AppConfiguration FromDbRecord(ConfigRecord record) => new(
        record.Id, 
        JsonDocument.Parse(record.Value.ToJson()).RootElement
    );
}

public static class JsonElementExtensions
{
    public static ConfigRecord ToDbRecord(this JsonElement element, string Id) => new(
        Id, 
        BsonDocument.Parse(element.ToString())
    );
}