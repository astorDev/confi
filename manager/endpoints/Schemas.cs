using System.Text.Json;
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
        await EnsureSchemaIsUpToDate(
            schemaCollection,
            nodeCandidate.Version,
            nodeCandidate.Schema,
            appId
        );
    }

    public static async Task<SchemeRecord> EnsureSchemaIsUpToDate(this IMongoCollection<SchemeRecord> schemaCollection, string candidateVersion, JsonElement candidateSchema, string appId)
    {
        var currentAppSchema = await schemaCollection.Search(appId);

        if (!Version.ImpliesUpdate(currentVersion: currentAppSchema?.Version, candidateVersion))
        {
            return currentAppSchema!;
        }

        var newSchema = new SchemeRecord(
            Id: appId,
            Version: candidateVersion,
            Schema: candidateSchema.ToBsonDocument()
        );

        await schemaCollection.Put(newSchema);
        return newSchema;
    }
}