using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPut("/config", async (JsonElement config) => {
    var client = new MongoClient("mongodb://localhost:27017");
    var database = client.GetDatabase("confi-playground");
    var collection = database.GetCollection<AppConfigurationDbRecord>("configurations");

    var result = await collection.ReplaceOneAsync(c => c.Id == "app", config.ToDbRecord("app"), new ReplaceOptions { IsUpsert = true });
    var saved = await collection.Find(c => c.Id == "app").FirstOrDefaultAsync();
    return AppConfiguration.FromDbRecord(saved);
});

app.Run();

public record AppConfigurationDbRecord(string Id, BsonDocument Value);

public record AppConfiguration(string Id, JsonElement Value)
{
    public static AppConfiguration FromDbRecord(AppConfigurationDbRecord record)
    {
        return new AppConfiguration(record.Id, JsonDocument.Parse(record.Value.ToJson()).RootElement);
    }
}

public static class JsonElementExtensions
{
    public static AppConfigurationDbRecord ToDbRecord(this JsonElement element, string Id)
    {
        return new AppConfigurationDbRecord(Id, BsonDocument.Parse(element.ToString()));
    }
}