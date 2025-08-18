using System.Collections;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Nist;
using Persic;

namespace Confi.Manager;

public static class AppVersionEndpoints
{
    public static IEndpointRouteBuilder MapAppVersionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(Uris.AppVersion("{appId}", "{version}"), PutAppVersion);
        endpoints.MapGet(Uris.LatestAppVersion("{appId}"), GetLatestAppVersion);
        endpoints.MapGet(Uris.AppVersion("{appId}", "{version}"), GetAppVersion);
        endpoints.MapGet(Uris.AppVersionConfiguration("{appId}", "{version}"), GetAppVersionConfiguration);
        endpoints.MapGet(Uris.AppVersionConfiguration("{appId}", Uris.Latest), GetAppLatestVersionConfiguration);
        endpoints.MapPut(Uris.AppVersionConfiguration("{appId}", "{version}"), PutAppVersionConfiguration);

        return endpoints;
    }

    public static async Task<AppVersion> PutAppVersion(
        string appId,
        string version,
        AppVersionCandidate candidate,
        IMongoCollection<AppVersionRecord> appVersionCollection
    )
    {
        var recordId = AppVersionRecord.BuildId(appId, version);
        var existingRecord = await appVersionCollection.Search(recordId);

        if (existingRecord != null) return existingRecord.ToProtocol();

        var config = await appVersionCollection.GetEffectiveConfiguration(candidate, appId);

        var newRecord = new AppVersionRecord(
            Id: recordId,
            AppId: appId,
            Version: version,
            Schema: candidate.Schema.ToBsonDoc(),
            Configuration: config.ToBsonDoc()
        );

        await appVersionCollection.InsertOneAsync(newRecord);
        return newRecord.ToProtocol();
    }

    public static async Task<AppVersion> GetAppVersion(
        string appId,
        string version,
        IMongoCollection<AppVersionRecord> appVersionCollection
    )
    {
        var recordId = AppVersionRecord.BuildId(appId, version);
        var record = await appVersionCollection.Search(recordId);

        if (record == null) throw new AppVersionNotFoundException(appId, version);

        return record.ToProtocol();
    }

    public static async Task<AppVersion> GetLatestAppVersion(
        string appId,
        IMongoCollection<AppVersionRecord> appVersionCollection
    )
    {
        var latestRecord = await appVersionCollection.LatestAppVersion(appId).Search();

        if (latestRecord == null) throw new AppNotFoundException(appId);

        return latestRecord.ToProtocol();
    }

    public static async Task<JsonElement> GetAppVersionConfiguration(
        string appId,
        string version,
        IMongoCollection<AppVersionRecord> appVersionCollection
    )
    {
        var recordId = AppVersionRecord.BuildId(appId, version);

        var record = await appVersionCollection.ById(recordId).SearchConfiguration();

        if (record == null) throw new AppVersionNotFoundException(appId, version);

        return record.ToJsonElement();
    }

    public static async Task<JsonElement> GetAppLatestVersionConfiguration(
        string appId,
        IMongoCollection<AppVersionRecord> appVersionCollection
    )
    {
        var record = await appVersionCollection.LatestAppVersion(appId).SearchConfiguration();

        if (record == default) throw new AppNotFoundException(appId);

        return record.ToJsonElement();
    }

    public static async Task<JsonElement> PutAppVersionConfiguration(
        string appId,
        string version,
        JsonElement configuration,
        IMongoCollection<AppVersionRecord> appVersionCollection
    )
    {
        var recordId = AppVersionRecord.BuildId(appId, version);

        var update = Builders<AppVersionRecord>.Update.Set(x => x.Configuration, configuration.ToBsonDoc());
        var result = await appVersionCollection.UpdateOneAsync(x => x.Id == recordId, update);
        if (result.MatchedCount == 0)
        {
            throw new AppVersionNotFoundException(appId, version);
        }

        return configuration;
    }

    public static Error? MapAppVersionErrors(Exception exception)
    {
        return exception switch
        {
            // TO DO: declare error in protocol
            AppVersionNotFoundException e => new(System.Net.HttpStatusCode.BadRequest, "AppVersionNotFound"),
            _ => null
        };
    }
}

public static class AppVersionCollectionExtensions
{
    public static IFindFluent<AppVersionRecord, AppVersionRecord> LatestAppVersion(this IMongoCollection<AppVersionRecord> collection, string appId)
    {
        return collection
            .Find(x => x.AppId == appId)
            .SortByDescending(x => x.Version)
            .Limit(1);
    }

    public static async Task<BsonDocument?> SearchConfiguration(this IFindFluent<AppVersionRecord, AppVersionRecord> filtered)
    {
        return await filtered.Project(x => x.Configuration).Search();
    }

    public static async Task<JsonElement> GetEffectiveConfiguration(
        this IMongoCollection<AppVersionRecord> appVersionCollection,
        AppVersionCandidate candidate,
        string appId
    )
    {
        var latestVersionConfiguration = await appVersionCollection.LatestAppVersion(appId).SearchConfiguration();
        if (latestVersionConfiguration == null) return candidate.Configuration;
        
        var jsonSchema = candidate.Schema.Deserialize<JsonSchema>(JsonSerializerOptions.Web)!;
        return jsonSchema.Combine(
            latestVersionConfiguration.ToJsonElement(),
            candidate.Configuration
        ).ToElement();
    }
}

public class AppVersionNotFoundException(string appId, string version) : Exception($"App version '{version}' for app '{appId}' not found.")
{
    public override IDictionary Data => new Dictionary<string, string>()
    {
        { "appId", appId },
        { "version", version }
    };
}