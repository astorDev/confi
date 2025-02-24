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
        endpoints.MapPut(
            "{appId}/nodes/{nodeId}", 
            async (
                string appId, 
                string nodeId, 
                NodeCandidate candidate, 
                IMongoCollection<NodeRecord> mongoCollection
            ) =>
            {
                var mongoRecord = new NodeRecord(
                    nodeId, 
                    appId, 
                    UpdatedAt: DateTime.UtcNow,
                    candidate.Schema.ToBsonDocument(), 
                    candidate.Configuration.ToBsonDocument()
                );

                await mongoCollection.Put(mongoRecord);
                return mongoRecord.ToProtocol();
            }
        );

        endpoints.MapGet(
            "{appId}/nodes/{nodeId}", 
            async (
                string appId, 
                string nodeId, 
                IMongoCollection<NodeRecord> mongoCollection
            ) =>
            {
                var mongoRecord = await mongoCollection
                    .Find(x => x.Id == nodeId && x.AppId == appId)
                    .FirstOrDefaultAsync() ?? throw new NodeNotFoundException(appId, nodeId);

                return mongoRecord.ToProtocol();
            }
        );

        return endpoints;
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

public record NodeRecord(
    string Id,
    string AppId,
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