using Nist;

namespace Confi.Manager;

public partial class Uris {
    public static string Apps = "apps";
    public static string App(string appId) => $"{Apps}/{appId}";
}

public record AppCandidate(
    JsonElement Schema,
    JsonElement Configuration,
    string Version
);

public record App(
    string Id,
    string Name,
    Dictionary<string, NodeState> Nodes,
    JsonElement Schema,
    JsonElement Configuration
);

public record AppSummary(
    string Id,
    string Name
);

public record AppCollection(
    int Count,
    AppSummary[] Items
);

public record AppChanges(
    string? Name
);

public partial class Client {
    public async Task<App> GetApp(string appId) 
        => await Get<App>(Uris.App(appId));

    public async Task<AppCollection> GetApps() => await Get<AppCollection>(Uris.Apps);
    public async Task<AppSummary> PatchApp(string appId, AppChanges changes) => await Patch<AppSummary>(Uris.App(appId), changes);
}

public partial class Errors
{
    public static Error AppNotFound => new(HttpStatusCode.BadRequest, "AppNotFound");
}