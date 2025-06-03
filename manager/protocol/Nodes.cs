using System.Text.Json;
using Nist;

namespace Confi.Manager;

public record Node(
    string Id,
    string AppId,
    string Version,
    JsonElement Schema,
    JsonElement Configuration
);

public record NodeCandidate(
    JsonElement Schema, 
    JsonElement Configuration,
    string Version
);

public record NodeState(string Status);

public class NodeStatus {
    public const string Synced = "synced";
    public const string NotSynced = "unsynced";
}

public partial class Uris {
    public static string Nodes = "nodes";
    public static string Node(string appId, string nodeId) => $"{App(appId)}/{Nodes}/{nodeId}";
}

public partial class Client {
    public async Task<Node> PutNode(string appId, string nodeId, NodeCandidate candidate) =>
        await Put<Node>(Uris.Node(appId, nodeId), candidate);

    public async Task<Node> GetNode(string appId, string nodeId) =>
        await Get<Node>(Uris.Node(appId, nodeId));
}

public partial class Errors {
    public static Error NodeNotFound = new (HttpStatusCode.BadRequest, $"NodeNotFound");
}