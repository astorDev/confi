using System.Collections;
using MongoDB.Bson;
using MongoDB.Driver;
using Nist;
using Persic;

namespace Confi.Manager;

public static class NodeEntrypoints
{
    public static IEndpointRouteBuilder MapNodes(this IEndpointRouteBuilder endpoints) 
    {
        endpoints.MapPut("{appId}/nodes/{nodeId}", PutNode);
        endpoints.MapGet("{appId}/nodes/{nodeId}", GetNode);

        return endpoints;
    }

    public static async Task<Node> GetNode(
        string appId, 
        string nodeId, 
        IMongoCollection<NodeRecord> mongoCollection)
    {
        var mongoRecord = await mongoCollection
            .Find(x => x.Id == nodeId && x.AppId == appId)
            .FirstOrDefaultAsync() ?? throw new NodeNotFoundException(appId, nodeId);

        return mongoRecord.ToProtocol();
    }

    public static async Task<Node> PutNode(
        string appId, 
        string nodeId, 
        NodeCandidate candidate, 
        IMongoCollection<NodeRecord> mongoCollection,
        IMongoCollection<SchemeRecord> schemasCollection)
    {
        var mongoRecord = new NodeRecord(
            nodeId, 
            appId, 
            candidate.Version,
            UpdatedAt: DateTime.UtcNow,
            candidate.Schema.ToBsonDocument(),  
            candidate.Configuration.ToBsonDocument()
        );

        await mongoCollection.Put(mongoRecord);

        var currentAppSchema = await mongoCollection
            .Find(x => x.Id == appId)
            .FirstOrDefaultAsync();

        if (currentAppSchema == null || candidate.Version.IsNewerThan(currentAppSchema.Version))
        {
            var newAppSchema = new SchemeRecord(
                appId, 
                candidate.Version, 
                candidate.Schema.ToBsonDocument()
            );

            await schemasCollection.Put(newAppSchema);
        }

        return mongoRecord.ToProtocol();
    }

    public static Error? MapNodesErrors(Exception exception)
    {
        return exception switch
        {
            NodeNotFoundException e => e.ToError(),
            _ => null
        };
    }
}

public static class VersionComparer
{
    public static bool IsNewerThan(this string candidate, string current)
    {
        return candidate.CompareTo(current) >= 0;
    }
}

public record NodeRecord(
    string Id,
    string AppId,
    string Version,
    DateTime UpdatedAt,
    BsonDocument Schema,
    BsonDocument Configuration
) : IMongoRecord<string>
{
    public Node ToProtocol()
    {
        return new Node(
            Id, 
            AppId, 
            Version,
            Schema.ToJsonElement(), 
            Configuration.ToJsonElement()
        );
    }
}

public class NodeNotFoundException(string appId, string nodeId) : Exception
{
    public override IDictionary Data => new Dictionary<string, string> {
        [ "appId" ] = appId,
        [ "nodeId" ] = nodeId
    };

    public Error ToError() => Errors.NodeNotFound;
}