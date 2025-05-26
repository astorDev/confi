using System.Collections;
using Microsoft.Extensions.Configuration;

namespace Confi;

public class FluentEnvironmentVariablesConfiguration
{
    public static IEnumerable<string> Keys(string rawKey)
    {
        yield return rawKey;

        var parts = rawKey.Split('_').Where(p => p != "").ToArray();

        for (var i = 1; i < parts.Length; i++)
        {
            var beforeColon = parts.Take(i);
            var afterColon = parts.Skip(i);

            yield return String.Join("", beforeColon) + ":" + String.Join("", afterColon);
        }

        yield return String.Join("", parts);
    }
    
    public class Source(string prefix) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(prefix);
    }

    public class Provider(string prefix) : ConfigurationProvider
    {
        public override void Load()
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
            {
                var key = (string)environmentVariable.Key;
                var value = (string?)environmentVariable.Value;

                var payload = PayloadFrom(key, value);
                foreach (var record in payload)
                {
                    Data.Add(record.Key, record.Value);
                }
            }
        }

        IEnumerable<KeyValuePair<string, string?>> PayloadFrom(string key, string? value)
        {
            if (prefix == "")
            {
                return Keys(key).Select(k => new KeyValuePair<string, string?>(k, value));
            }

            if (key.StartsWith(prefix))
            {
                return Keys(key.Replace(prefix, "")).Select(k => new KeyValuePair<string, string?>(k, value));
            }

            return Array.Empty<KeyValuePair<string, string?>>();
        }
    }
}

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddFluentEnvironmentVariables(this IConfigurationBuilder builder, string prefix = "") =>
        builder.Add(new FluentEnvironmentVariablesConfiguration.Source(prefix));
}