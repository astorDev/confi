using System.Reflection;
using Confi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class Extensions 
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new MissingConfigurationKeyException(key);
    }

    public static string GetRequiredConfigurationValue(IServiceProvider serviceProvider, string configurationKey)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return configuration.GetRequiredValue(configurationKey);
    }
}

namespace Confi
{
    public class MissingConfigurationKeyException(string key) : Exception(message: $"Unable to find configuration value by key `{key}`")
    {
    }
}
