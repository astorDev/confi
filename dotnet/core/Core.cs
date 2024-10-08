using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confi;

public static class ConfigurationExtensions 
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new MissingConfigurationKeyException(key);
    }

    public static string GetRequiredConfigurationValue(this IServiceProvider serviceProvider, string configurationKey)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return configuration.GetRequiredValue(configurationKey);
    }
}

public class MissingConfigurationKeyException(string key) : Exception(message: $"Unable to find configuration value by key `{key}`")
{
}