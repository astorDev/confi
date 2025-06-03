using System.Text.Json;
using System.Text.Json.Nodes;

namespace Confi.Manager.Consumer;

public static class JsonConvertationHelper
{
    public static JsonElement AsJsonElement(this JsonNode jsonNode)
    {
        var rawJson = jsonNode.ToJsonString();
        return JsonDocument.Parse(rawJson).RootElement;
    }

    public static JsonElement AsJsonElement(this JsonObject jsonObj)
    {
        var rawJson = jsonObj.ToJsonString();
        return JsonDocument.Parse(rawJson).RootElement;
    }
}