using System.Text.Json;
using System.Text.Json.Nodes;

namespace Confi.Manager;

public static class JsonNodeExtensions
{
    public static JsonElement ToElement(this JsonNode node) => JsonSerializer.SerializeToElement(node);
}

public record JsonSchema(
    string Type,
    Dictionary<string, JsonSchema> Properties,
    string[] Required
)
{
    public JsonNode GetFirstRawValue(string propertyName, IEnumerable<JsonElement> elements)
    {
        foreach (var element in elements)
        {
            if (element.TryGetProperty(propertyName, out var value))
            {
                return JsonNode.Parse(value.GetRawText())!;
            }
        }

        throw new KeyNotFoundException($"Property '{propertyName}' not found in any of the provided elements.");
    }

    public JsonNode Combine(params IEnumerable<JsonElement> elements)
    {
        var builder = new JsonObject();

        foreach (var property in Properties)
        {
            builder[property.Key] = GetNode(property, elements);
        }

        return builder;
    }

    public JsonNode GetNode(KeyValuePair<string, JsonSchema> property, IEnumerable<JsonElement> elements)
    {
        switch (property.Value.Type)
        {
            case "object":
                var subcandidates = elements.Select(element =>
                {
                    var found = element.TryGetProperty(property.Key, out var value);
                    return (found, value);
                })
                .Where(x => x.found)
                .Select(x => x.value!);

                return property.Value.Combine(subcandidates);
            case "array":
                throw new NotSupportedException("Array properties are not supported yet.");
            default:
                return GetFirstRawValue(property.Key, elements);
        }
    }
}