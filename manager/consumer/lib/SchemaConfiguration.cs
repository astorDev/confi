using System.Text.Json;
using System.Text.Json.Nodes;
using Confi.Manager;
using Microsoft.Extensions.Configuration;

namespace Confi;

public static class SchemaConfigurationExtensions
{
    public static JsonNode RetrieveJson(this IConfiguration configuration, JsonSchema schema)
    {
        JsonObject builder = [];

        foreach (var property in schema.Properties ?? [])
        {
            builder[property.Key] = property.Value.Type switch
            {
                JsonSchemaType.String => configuration[property.Key],
                JsonSchemaType.Integer => configuration.GetOptionalInt(property.Key),
                JsonSchemaType.Object => configuration.GetSection(property.Key).RetrieveJson(property.Value),
                _ => throw new NotSupportedException($"Unsupported schema type: {property.Value.Type}"),
            };
        }

        return builder;
    }

    public static int? GetOptionalInt(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (value == null) return null;
        if (!int.TryParse(value, out var result)) throw new FormatException($"Configuration value for '{key}' is not a valid integer: '{value}'");

        return result;
    }
}

public static class JsonSchemaParser
{
    public static JsonSchema ParseAsJsonSchema(this string schemaString)
    {
        return JsonSerializer.Deserialize<JsonSchema>(schemaString, JsonSerializerOptions.Web) 
               ?? throw new InvalidOperationException("Failed to deserialize schema.");
    }
}

public class JsonSchemaType
{
    public const string String = "string";
    public const string Integer = "integer";
    public const string Object = "object";
}