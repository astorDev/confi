using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

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

    public static async Task<TMongoRecord?> Search<TMongoRecord>(this IMongoCollection<TMongoRecord> collection, string id)
        where TMongoRecord : IMongoRecord<string>
    {
        return await collection.Find(r => r.Id == id).FirstOrDefaultAsync();
    }
}