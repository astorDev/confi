using System.Collections;
using MongoDB.Bson;
using MongoDB.Driver;
using Nist;
using Persic;

namespace Confi.Manager;

public static class AppEntrypoints
{
    public static IEndpointRouteBuilder MapApps(this IEndpointRouteBuilder endpoints) 
    {
        endpoints.MapGet("{appId}", GetApp);

        return endpoints;
    }

    public static App GetApp(
        string appId, 
        IMongoCollection<SchemeRecord> schemasCollection,
        IMongoCollection<NodeRecord> nodesCollection)
    {
        var schemeRecord = schemasCollection
            .Find(x => x.Id == appId)
            .FirstOrDefault() ?? throw new AppNotFoundException(appId);

        var nodeRecords = nodesCollection
            .Find(x => x.AppId == appId)
            .ToList();

        return new App(
            schemeRecord.Id,
            nodeRecords.ToDictionary(
                x => x.Id, 
                x => new NodeState(
                    Status: x.DetermineStatus(schemeRecord.Schema)
                )
            ),
            schemeRecord.Schema.ToJsonElement()
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

public static class NodeStatusDeterminer
{
    public static string DetermineStatus(this NodeRecord node, BsonDocument configuration)
    {
        return node.Schema.Equivalent(configuration) ? NodeStatus.Synced : NodeStatus.NotSynced;
    }
}

public class AppNotFoundException(string appId) : Exception
{
    public override IDictionary Data => new Dictionary<string, string> {
        { "appId", appId }
    };

    public Error ToError() => Errors.AppNotFound;
}

public record SchemeRecord(
    string Id, // matches app id
    string Version,
    BsonDocument Schema
) : IMongoRecord<string>;