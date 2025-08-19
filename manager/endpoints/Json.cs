using System.Text.Json;
using System.Text.Json.Nodes;

namespace Confi.Manager;

public static class JsonExtensions
{
    public static bool DeepEquals(this JsonSchema schema, JsonSchema other)
    {
        var schemaSafe = schema.CopyWithEmptyCollections();
        var otherSafe = other.CopyWithEmptyCollections();

        return DeepEqualsUnsafe(schemaSafe, otherSafe);
    }

    private static bool DeepEqualsUnsafe(this JsonSchema schema, JsonSchema other)
    {
        if (schema.Type != other.Type)
            return false;

        if (!schema.Required!.SequenceEqual(other.Required!))
            return false;

        if (!schema.Properties!.Keys.SequenceEqual(other.Properties!.Keys))
              return false;

        foreach (var property in schema.Properties!)
        {
            if (!other.Properties!.TryGetValue(property.Key, out var otherProperty) ||
                !property.Value.DeepEquals(otherProperty))
            {
                return false;
            }
        }

        return true;
    }

    public static JsonElement ToElement(this JsonNode node) => JsonSerializer.SerializeToElement(node);

    public static JsonNode Combine(this JsonSchema schema, params IEnumerable<JsonElement> elements)
    {
        var builder = new JsonObject();

        foreach (var property in schema.Properties ?? [])
        {
            builder[property.Key] = property.GetNode(elements);
        }

        return builder;
    }

    public static JsonNode GetFirstRawValue(this IEnumerable<JsonElement> elements, string propertyName)
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

    public static JsonNode GetNode(this KeyValuePair<string, JsonSchema> property, IEnumerable<JsonElement> elements)
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
                return elements.GetFirstRawValue(property.Key);
        }
    }
}