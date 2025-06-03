using System.Text.Json;
using System.Text.Json.Nodes;

namespace Confi.Manager.Consumer;

public class JsonSchema(JsonNode inner)
{
    public const string AdditionalPropertiesKey = "additionalProperties";
    public const string PropertiesKey = "properties";

    private Dictionary<string, JsonSchema>? properties;
    public Dictionary<string, JsonSchema> Properties => properties ??= inner.GetPropertiesDictionary();

    public JsonSchema? AdditionalProperties => Optional(inner[AdditionalPropertiesKey]);
    public static JsonSchema? Optional(JsonNode? inner) => inner == null ? null : new JsonSchema(inner);

    public string Type => inner.GetSchemaType();

    public static JsonSchema FromFile(string filePath)
    {
        var schemaString = File.ReadAllText(filePath);
        var schemaNode = JsonNode.Parse(schemaString) ?? throw new("Failed to parse JSON schema");
        return new JsonSchema(schemaNode);
    }

    public static JsonSchema Parse(string json)
    {
        var schemaNode = JsonNode.Parse(json) ?? throw new("Failed to parse JSON schema");
        return new JsonSchema(schemaNode);
    }

    public JsonElement AsJsonElement()
    {
        var rawJson = inner.ToJsonString();
        return JsonDocument.Parse(rawJson).RootElement;
    }
}

public static class JsonSchemaExtensions
{
    public const string Required = "required";
    public const string Type = "type";

    public static string GetSchemaType(this JsonNode schema)
    {
        try
        {
            var type = schema[Type] ?? throw new InvalidOperationException("Schema does not contain a 'type' property.");
            return type.GetValue<string>();
        }
        catch (Exception ex)
        {
            throw new($"Schema under'{schema.GetPath()}' does not contain a valid 'type'", ex);
        }
    }

    public static IEnumerable<(string Key, JsonSchema SubSchema)> GetProperties(this JsonNode inner)
    {
        var properties = inner[JsonSchema.PropertiesKey];
        foreach (var property in properties?.AsObject() ?? [])
        {
            if (property.Key == Required) continue;
            var propertyValue = property.Value ?? throw new("Schema property value cannot be null");
            yield return (property.Key, new JsonSchema(propertyValue));
        }
    }

    public static Dictionary<string, JsonSchema> GetPropertiesDictionary(this JsonNode inner)
    {
        return inner.GetProperties().ToDictionary(x => x.Key, x => x.SubSchema);
    }
}