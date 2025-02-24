using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

namespace Confi.Manager;

public record SchemeRecord(
    string Id, // matches app id
    string Version,
    BsonDocument Schema
) : IMongoRecord<string>;

public static class SchemaHelper
{
    public static async Task EnsureSchemaIsUpToDate(this IMongoCollection<SchemeRecord> schemaCollection, NodeCandidate nodeCandidate, string appId)
    {
        var currentAppSchema = await schemaCollection.Search(appId);

        if (currentAppSchema == null || currentAppSchema.Version != nodeCandidate.Version)
        {
            var newSchema = new SchemeRecord(
                Id: appId,
                Version: nodeCandidate.Version,
                Schema: nodeCandidate.Schema.ToBsonDocument()
            );

            await schemaCollection.Put(newSchema);
        }
    }
}