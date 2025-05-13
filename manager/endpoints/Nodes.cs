using System.Collections;
using MongoDB.Bson;
using MongoDB.Driver;
using Nist;
using Persic;

namespace Confi.Manager;

public static class NodeHelper
{
    public static IEndpointRouteBuilder MapNodes(this IEndpointRouteBuilder endpoints) 
    {
        endpoints.MapPut("{appId}/nodes/{nodeId}", PutNode);
        endpoints.MapGet("{appId}/nodes/{nodeId}", GetNode);
        endpoints.MapDelete("{appId}/nodes/{nodeId}", DeleteNode);

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
        IMongoCollection<NodeRecord> nodeCollection,
        IMongoCollection<SchemeRecord> schemasCollection,
        IMongoCollection<ConfigurationRecord> configurationCollection)
    {
        var record = new NodeRecord(
            nodeId, 
            appId, 
            candidate.Version,
            UpdatedAt: DateTime.UtcNow,
            candidate.Schema.ToBsonDocument(),  
            candidate.Configuration.ToBsonDocument()
        );

        await nodeCollection.Put(record);

        await schemasCollection.EnsureSchemaIsUpToDate(candidate, appId);
        await configurationCollection.GetOrSetAppConfiguration(appId, candidate.Configuration);

        return record.ToProtocol();
    }

    public static async Task<Node> DeleteNode(
        string appId,
        string nodeId,
        IMongoCollection<NodeRecord> nodeCollection
    )
    {
        var node = await nodeCollection.Search(nodeId) ?? throw new NodeNotFoundException(appId, nodeId);

        await nodeCollection.DeleteOneAsync(x => x.Id == nodeId && x.AppId == appId);

        return node.ToProtocol();
    }

    public static Error? MapNodesErrors(Exception exception)
    {
        return exception switch
        {
            NodeNotFoundException e => e.ToError(),
            _ => null
        };
    }
    
    public static string DetermineStatus(this NodeRecord node, BsonDocument configuration)
    {
        return node.Configuration.Equivalent(configuration)
            ? NodeStatus.Synced : NodeStatus.NotSynced;
    }
}

public static class VersionComparer
{
    public static async Task<List<NodeRecord>> ListBy(this IMongoCollection<NodeRecord> mongoCollection, string appId)
    {
        return await mongoCollection
            .Find(x => x.AppId == appId)
            .ToListAsync();
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