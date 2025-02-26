using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using Persic;

namespace Confi;

public class MongoConfigurationBuilder(IServiceCollection services)
{
    public MongoConfigurationBuilder AddLoader(string configurationKey, MongoLoadingMode loadingMode = MongoLoadingMode.CollectionWatch)
    {
        services.AddMongoBackgroundConfigurationLoader(configurationKey, loadingMode);
        return this;
    }
}

public static class MongoConfigurationExtensions
{
    public static IHostApplicationBuilder AddMongoConfiguration(
        this IHostApplicationBuilder builder,
        Action<MongoConfigurationBuilder> configure,
        string configsCollectionName = "configs"
        )
    {
        builder.Configuration.AddBackgroundStore(MongoConfigurationLoader.Key);

        builder.Services.AddBackgroundConfigurationStores();

        builder.Services.AddMongoCollection<ConfigRecord>(configsCollectionName);

        var loadersBuilder = new MongoConfigurationBuilder(builder.Services);
        configure(loadersBuilder);

        return builder;
    }

    public static Dictionary<string, object> ToConfigurationDictionary(this BsonDocument document, string prefix = "")
    {
        var configs = new Dictionary<string, object>();
        configs.EnrichFromBsonDocument(document);
        return configs;
    }

    private static void EnrichFromBsonDocument(this Dictionary<string, object> configs, BsonDocument document, string prefix = "")
    {
        foreach (var pair in document)
        {
            if (pair.Value.IsBsonDocument)
            {
                configs.EnrichFromBsonDocument(pair.Value.AsBsonDocument, prefix + pair.Name + ":");
            }
            else if (pair.Value.IsBsonArray)
            {
                var array = pair.Value.AsBsonArray;
                for (var i = 0; i < array.Count; i++)
                {
                    configs.EnrichFromBsonDocument(array[i].AsBsonDocument, prefix + pair.Name + ":" + i + ":");
                }
            }
            else
            {
                configs[prefix + pair.Name] = pair.Value.ToJson();
            }
        }
    }
}