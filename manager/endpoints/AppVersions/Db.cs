using MongoDB.Bson;
using Persic;

namespace Confi.Manager;

public record AppVersionRecord(
    string Id,
    string AppId,
    string Version,
    JsonSchema Schema,
    BsonDocument Configuration,
    DateTime CreationTime
) : IMongoRecord<string>
{
    public static string BuildId(string appId, string version) => $"{appId}/{version}";

    public AppVersion ToProtocol()
    {
        return new AppVersion(
            AppId: AppId,
            Version: Version,
            Schema: Schema,
            Configuration: Configuration.ToJsonElement(),
            CreationTime: CreationTime
        );
    }
}