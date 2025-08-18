using MongoDB.Bson;
using Persic;

namespace Confi.Manager;

public record AppVersionRecord(
    string Id,
    string AppId,
    string Version,
    BsonDocument Schema,
    BsonDocument Configuration
) : IMongoRecord<string>
{
    public static string BuildId(string appId, string version) => $"{appId}/{version}";

    public AppVersion ToProtocol()
    {
        return new AppVersion(
            AppId: AppId,
            Version: Version,
            Schema: Schema.ToJsonElement(),
            Configuration: Configuration.ToJsonElement()
        );
    }
}