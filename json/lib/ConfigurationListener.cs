using Microsoft.Extensions.Configuration;

namespace Confi;

public class ConfigurationListener : ConfigurationProvider
{
    public ConfigurationListener(IListenable<IDictionary<string, string?>> updatedListenable)
    {
        updatedListenable.AddListener(config =>
        {
            Data = config;
            OnReload();
        });
    }
}