using System.Text.Json;
using MongoDB.Bson;

namespace Confi;

public static class Mongo
{
    public static async Task<JsonDocument> ToJsonDocument(this Task<BsonDocument> bsonTask)
    {
        var bson = await bsonTask;
        return JsonDocument.Parse(bson.ToJson());
    }

    public static JsonElement ToJsonElement(this BsonDocument bson) => 
        JsonDocument.Parse(bson.ToJson()).RootElement;

    public static BsonDocument ToBsonDocument(this JsonElement json) =>
        BsonDocument.Parse(json.GetRawText());

    public static bool Equivalent(this BsonDocument bson, BsonDocument other) => 
        bson.ToJson() == other.ToJson();
}