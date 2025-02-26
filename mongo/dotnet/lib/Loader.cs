using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

namespace Confi;

public record ConfigurationRecord(string Id, BsonDocument Value) : IMongoRecord<string>
{
    public Dictionary<string, object> ToConfigurationDictionary()
    {
        var configs = new Dictionary<string, object>();
        EnrichFromBsonDocument(configs, Value);
        return configs;
    }

    private static void EnrichFromBsonDocument(Dictionary<string, object> configs, BsonDocument document, string prefix = "")
    {
        foreach (var pair in document)
        {
            if (pair.Value.IsBsonDocument)
            {
                EnrichFromBsonDocument(configs, pair.Value.AsBsonDocument, prefix + pair.Name + ":");
            }
            else if (pair.Value.IsBsonArray)
            {
                var array = pair.Value.AsBsonArray;
                for (var i = 0; i < array.Count; i++)
                {
                    EnrichFromBsonDocument(configs, array[i].AsBsonDocument, prefix + pair.Name + ":" + i + ":");
                }
            }
            else
            {
                configs[prefix + pair.Name] = pair.Value.ToJson();
            }
        }
    }
}

public class MongoBackgroundConfigurationLoader(
    IMongoCollection<ConfigurationRecord> collection, 
    ConfigurationBackgroundStore.Factory factory,
    string documentId,
    ILogger<MongoBackgroundConfigurationLoader> logger
)
{
    public const string Key = "mongo";

    private readonly ConfigurationBackgroundStore store = factory.GetStore(Key);
    public IMongoCollection<ConfigurationRecord> Collection { get; } = collection;
    public string DocumentId { get; } = documentId;
    public ILogger<MongoBackgroundConfigurationLoader> Logger { get; } = logger;

    public string CollectionName => Collection.CollectionNamespace.CollectionName;

    public void Upload(ConfigurationRecord configurationRecord)
    {
        store.SetAll(configurationRecord.ToConfigurationDictionary());
    }

    public async Task<ConfigurationRecord?> SearchAsync(CancellationToken cancellationToken)
    {
        return await Collection.Find(x => x.Id == DocumentId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}

public static class LoaderRegistration
{
    public static IServiceCollection AddMongoBackgroundConfigurationLoader(
        this IServiceCollection services, 
        string documentId, 
        Func<MongoBackgroundConfigurationLoader, IHostedService> factory
        )
    {
        return services.AddSingleton(sp => {
            var collection = sp.GetRequiredService<IMongoCollection<ConfigurationRecord>>();
            var configurationFactory = sp.GetRequiredService<ConfigurationBackgroundStore.Factory>();
            var logger = sp.GetRequiredService<ILogger<MongoBackgroundConfigurationLoader>>();

            var uploader = new MongoBackgroundConfigurationLoader(collection, configurationFactory, documentId, logger);

            return factory(uploader);
        });   
    }
}