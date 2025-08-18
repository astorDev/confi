using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

namespace Confi;

public static class Mongo
{
    public static JsonElement ToJsonElement(this BsonDocument bson) =>
        JsonDocument.Parse(bson.ToJson()).RootElement;

    public static BsonDocument ToBsonDocument(this JsonElement json) =>
        BsonDocument.Parse(json.GetRawText());

    public static bool Equivalent(this BsonDocument bson, BsonDocument other) =>
        bson.ToJson() == other.ToJson();

    public static async Task<TMongoRecord?> Search<TMongoRecord>(this IMongoCollection<TMongoRecord> collection, string id)
        where TMongoRecord : IMongoRecord<string> =>
        await collection.Find(r => r.Id == id).FirstOrDefaultAsync();

    public static async Task<TResult[]> ToArray<T, TResult>(this IMongoCollection<T> collection, Func<T, TResult> selector) where T : class
    {
        var list = await collection.Find(_ => true).ToListAsync();
        return [.. list.Select(selector)];
    }

    public static async Task<Dictionary<TKey, TValue>> ToDictionary<T, TKey, TValue>(this IMongoCollection<T> collection, Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where T : class where TKey : notnull
    {
        var list = await collection.Find(_ => true).ToListAsync();
        return list.ToDictionary(keySelector, valueSelector);
    }

    public static async Task<UpdateResult> Put<T>(this IMongoCollection<T> collection, string id, UpdateDefinition<T> update) where T : IMongoRecord<string>
        => await collection.UpdateOneAsync(x => x.Id == id, update, new UpdateOptions { IsUpsert = true });
}