using System.Collections;
using MongoDB.Driver;
using Nist;

namespace Confi.Manager;

public static class AppEndpoints
{
    public static IEndpointRouteBuilder MapApps(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Uris.App("{appId}"), GetApp);
        endpoints.MapPut(Uris.App("{appId}"), PutApp);

        // backward-compatibility
        endpoints.MapGet("{appId}", GetApp);
        endpoints.MapPut("{appId}", PutApp);

        return endpoints;
    }

    public static async Task<App> PutApp(
        string appId,
        AppCandidate candidate,
        IMongoCollection<SchemeRecord> schemaCollection,
        IMongoCollection<ConfigurationRecord> configurationCollection,
        IMongoCollection<NodeRecord> nodeCollection
    )
    {
        var schemeRecord = await schemaCollection.EnsureSchemaIsUpToDate(
            candidate.Version,
            candidate.Schema,
            appId
        );

        var configurationRecord = await configurationCollection.GetOrSetAppConfiguration(
            appId,
            candidate.Configuration
        );

        var nodeRecords = await nodeCollection.ListBy(
            appId: appId
        );

        var app = AppAssembler.Assemble(
            schemeRecord,
            configurationRecord,
            nodeRecords
        );

        return app;
    }

    public static async Task<App> GetApp(
        string appId,
        IMongoCollection<SchemeRecord> schemasCollection,
        IMongoCollection<NodeRecord> nodesCollection,
        IMongoCollection<ConfigurationRecord> configurationsCollection)
    {
        var schemeRecord = await schemasCollection.Search(appId)
            ?? throw new AppNotFoundException(appId);

        var configurationRecord = await configurationsCollection.Search(appId)
            ?? throw new AppNotFoundException(appId);

        var nodeRecords = await nodesCollection
            .Find(x => x.AppId == appId)
            .ToListAsync();

        return AppAssembler.Assemble(
            schemeRecord,
            configurationRecord,
            nodeRecords
        );
    }

    public static Error? MapAppErrors(Exception exception)
    {
        return exception switch
        {
            AppNotFoundException e => e.ToError(),
            _ => null
        };
    }
}

public class AppAssembler
{
    public static App Assemble(SchemeRecord schemeRecord, ConfigurationRecord configurationRecord, IEnumerable<NodeRecord> nodeRecords)
    {
        return new App(
            schemeRecord.Id,
            nodeRecords.ToDictionary(
                x => x.Id,
                x => new NodeState(
                    Status: x.DetermineStatus(configurationRecord.Value)
                )
            ),
            schemeRecord.Schema.ToJsonElement(),
            configurationRecord.Value.ToJsonElement()
        );
    }

}

public class AppNotFoundException(string appId) : Exception
{
    public override IDictionary Data => new Dictionary<string, string> {
        { "appId", appId }
    };

    public Error ToError() => Errors.AppNotFound;
}