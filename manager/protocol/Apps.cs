using System.Text.Json;
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
    Dictionary<string, NodeState> Nodes,
    JsonElement Schema,
    JsonElement Configuration
);

public partial class Client {
    public async Task<App> GetApp(string appId) 
        => await Get<App>(Uris.App(appId));
}

public partial class Errors
{
    public static Error AppNotFound => new(HttpStatusCode.BadRequest, "AppNotFound");
}