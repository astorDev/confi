using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

namespace Confi.Manager;

public record ConfigurationRecord(
    string Id,
    BsonDocument Value
) : IMongoRecord<string>;

public static class ConfigurationHelper
{
    public static IEndpointRouteBuilder MapConfiguration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("{appId}/configuration", PutConfiguration);

        return endpoints;
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

        return await AppHelper.GetApp(
            appId,
            schemaCollection,
            nodesCollection,
            configurationsCollection
        );
    }

    public static async Task EnsureAppConfigurationIsSet(
        this IMongoCollection<ConfigurationRecord> mongoCollection,
        string appId,
        BsonDocument configuration)
    {
        var existing = await mongoCollection.Search(appId);

        if (existing == null)
        {
            await mongoCollection.Put(new ConfigurationRecord(
                Id: appId,
                Value: configuration
            ));
        }
    }
}