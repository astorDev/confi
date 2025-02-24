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
}