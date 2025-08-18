using Confi.Manager;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

namespace Confi;

public record NodeRecord(
    string Id,
    string AppId,
    string Version,
    DateTime UpdatedAt,
    BsonDocument Schema,
    BsonDocument Configuration,
    DateTime ExpiresAt
) : IMongoRecord<string>
{
    public class CollectionConfiguration(IMongoCollection<NodeRecord> collection) : IMongoCollectionConfiguration
    {
        public async Task Apply()
        {
            await collection.Indexes.CreateTimeToLive(x => x.ExpiresAt);
        }
    }

    public static NodeRecord From(string appId, string nodeId, NodeCandidate candidate)
    {
        return new NodeRecord(
            nodeId,
            appId,
            candidate.Version,
            UpdatedAt: DateTime.UtcNow,
            candidate.Schema.ToBsonDoc(),
            candidate.Configuration.ToBsonDoc(),
            ExpiresAt: DateTime.UtcNow.Add(TimeSpan.FromSeconds(10) /* TO DO: replace with time to live after nuget update*/ )
        );
    }

    public Node ToProtocol()
    {
        return new Node(
            Id,
            AppId,
            Version,
            Schema.ToJsonElement(),
            Configuration.ToJsonElement(),
            ExpiresAt
        );
    }
}

public static class NodeDbRegistrationExtensions
{
    public static MongoRegistrationBuilder AddNodeCollection(this MongoRegistrationBuilder builder)
    {
        builder.AddCollection<NodeRecord, NodeRecord.CollectionConfiguration>("nodes");
        
        return builder;
    }
}