global using System.Text.Json;

namespace Confi.Manager;

public partial class Uris
{
    public const string Versions = "versions";
    public const string Latest = "latest";

    public static string AppVersion(string appId, string version) => $"{Apps}/{appId}/{Versions}/{version}";
    public static string LatestAppVersion(string appId) => AppVersion(appId, Latest);
    public static string AppVersionConfiguration(string appId, string version) => $"{AppVersion(appId, version)}/{Configuration}";
    public static string AppLatestVersionConfiguration(string appId) => $"{LatestAppVersion(appId)}/{Configuration}";
}

public record AppVersionCandidate(
    JsonElement Schema,
    JsonElement Configuration
);

public record AppVersion(
    string AppId,
    string Version,
    JsonElement Schema,
    JsonElement Configuration
);

public partial class Client
{
    public async Task<AppVersion> PutAppVersion(string appId, string version, AppVersionCandidate candidate)
        => await Put<AppVersion>(Uris.AppVersion(appId, version), candidate);

    public async Task<AppVersion> GetAppVersion(string appId, string version)
        => await Get<AppVersion>(Uris.AppVersion(appId, version));

    public async Task<AppVersion> GetLatestAppVersion(string appId)
        => await Get<AppVersion>(Uris.LatestAppVersion(appId));

    public async Task<JsonElement> GetAppVersionConfiguration(string appId, string version)
        => await Get<JsonElement>(Uris.AppVersionConfiguration(appId, version));

    public async Task<JsonElement> PutAppVersionConfiguration(string appId)
        => await Put<JsonElement>(Uris.AppVersionConfiguration(appId, Uris.Latest), new JsonElement());
}