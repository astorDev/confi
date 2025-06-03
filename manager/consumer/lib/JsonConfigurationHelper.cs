using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace Confi.Manager.Consumer;

public static class JsonConfigurationHelper
{
    public static JsonObject AsJson(this IConfiguration configuration, JsonSchema schema)
    {
        var result = new JsonObject();

        foreach (var section in configuration.GetChildren())
        {
            if (schema.AdditionalProperties != null)
            {
                result[section.Key] = section.AsJsonNode(schema.AdditionalProperties);
            }
            else if (schema.Properties.TryGetValue(section.Key, out JsonSchema? subschema))
            {
                result[section.Key] = section.AsJsonNode(subschema);
            }
        }

        return result;
    }

    public static JsonNode AsJsonNode(this IConfigurationSection section, JsonSchema schema)
    {
        return schema.Type switch
        {
            "string" => (JsonNode)section.AsString(),
            "number" or "integer" => (JsonNode)section.AsDecimal(),
            "boolean" => (JsonNode)section.AsBoolean(),
            "object" => section.AsJson(schema),
            _ => throw new NotSupportedException($"Unsupported type '{schema.Type}' for section '{section.Path}'"),
        };
    }

    public static decimal AsDecimal(this IConfigurationSection section)
    {
        if (decimal.TryParse(section.Value, out var number)) return number;

        throw new FormatException($"Invalid decimal format for section '{section.Path}': {section.Value}");
    }

    public static bool AsBoolean(this IConfigurationSection section)
    {
        if (bool.TryParse(section.Value, out var boolean)) return boolean;
        throw new FormatException($"Invalid boolean format for section '{section.Path}': {section.Value}");
    }

    public static string AsString(this IConfigurationSection section)
    {
        return section.Value ?? throw new FormatException($"Invalid string format for section '{section.Path}': {section.Value}");
    }
}
