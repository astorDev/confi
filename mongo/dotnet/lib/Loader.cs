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
                configs[prefix + pair.Name] = pair.Value.ToString()!;
            }
        }
    }
}

public class MongoConfigurationLoader(
    IMongoCollection<ConfigurationRecord> collection, 
    ConfigurationBackgroundStore.Factory factory,
    string documentId,
    ILogger<MongoConfigurationLoader> logger
)
{
    private const string keyPrefix = "mongo";
    public static string Key(string documentId) => $"{keyPrefix}:{documentId}";

    private readonly ConfigurationBackgroundStore store = factory.GetStore(Key(documentId));

    public void Upload(ConfigurationRecord configurationRecord)
    {
        store.SetAll(configurationRecord.ToConfigurationDictionary());
    }
    
    public IMongoCollection<ConfigurationRecord> Collection { get; } = collection;
    public string DocumentId { get; } = documentId;
    public ILogger<MongoConfigurationLoader> Logger { get; } = logger;

    public string CollectionName => Collection.CollectionNamespace.CollectionName;

    public async Task<ConfigurationRecord?> SearchAsync(CancellationToken cancellationToken)
    {
        return await Collection.Find(x => x.Id == DocumentId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public class Factory(
        IMongoCollection<ConfigurationRecord> collection,
        ConfigurationBackgroundStore.Factory configurationFactory,
        ILogger<MongoConfigurationLoader> logger
    )
    {
        public MongoConfigurationLoader GetLoader(string documentId)
        {
            return new MongoConfigurationLoader(collection, configurationFactory, documentId, logger);
        }
    }
}