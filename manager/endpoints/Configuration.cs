using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

namespace Confi.Manager;

public record ConfigurationRecord(
    string Id,
    BsonDocument Value
) : IMongoRecord<string>;

public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfiguration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(Uris.AppConfiguration("{appId}"), PutConfiguration);
        endpoints.MapGet(Uris.AppConfiguration("{appId}"), GetConfiguration);

        // backward-compatibility
        endpoints.MapPut("{appId}/configuration", PutConfiguration);

        return endpoints;
    }

    public static async Task<JsonDocument> GetConfiguration(string appId, IMongoCollection<ConfigurationRecord> configurationsCollection
    )
    {
        var configuration = await configurationsCollection.Search(appId);
        if (configuration == null) throw new AppNotFoundException(appId);

        return configuration.Value.ToJsonDocument();
    }

    public static async Task<App> PutConfiguration(
        string appId,
        JsonElement configuration,
        IMongoCollection<ConfigurationRecord> configurationsCollection,
        IMongoCollection<SchemeRecord> schemaCollection,
        IMongoCollection<NodeRecord> nodesCollection
    )
    {
        await configurationsCollection.Put(new ConfigurationRecord(
            Id: appId,
            Value: configuration.ToBsonDocument()
        ));

        return await AppEndpoints.GetApp(
            appId,
            schemaCollection,
            nodesCollection,
            configurationsCollection
        );
    }

    public static async Task<ConfigurationRecord> GetOrSetAppConfiguration(
        this IMongoCollection<ConfigurationRecord> mongoCollection,
        string appId,
        JsonElement configuration)
    {
        var existing = await mongoCollection.Search(appId);
        if (existing != null) return existing;

        var set = new ConfigurationRecord(
            Id: appId,
            Value: configuration.ToBsonDocument()
        );

        await mongoCollection.Put(set);

        return set;
    }
}