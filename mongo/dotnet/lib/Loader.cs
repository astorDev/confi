using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

namespace Confi;

public record ConfigRecord(string Id, BsonDocument Value) : IMongoRecord<string>;

public class MongoConfigurationLoader(
    IMongoCollection<ConfigRecord> collection, 
    ConfigurationBackgroundStore.Factory factory,
    string documentId,
    ILogger<MongoConfigurationLoader> logger) : BackgroundService
{
    public const string Key = "mongo";
    private readonly ConfigurationBackgroundStore store = factory.GetStore(Key);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await LoadInitialConfigurationAsync(stoppingToken);
                await RunWatchingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while watching for changes");
                await Task.Delay(500, stoppingToken);
            }
        }
    }

    private async Task LoadInitialConfigurationAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Loading initial configuration from collection {collectionName} for document {documentId}", 
            collection.CollectionNamespace.CollectionName,
            documentId
        );

        var document = await collection.Find(x => x.Id == documentId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (document is not null)
        {
            store.SetAll(document.Value.ToConfigurationDictionary());
        }
    }

    private async Task RunWatchingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting watching collection {collectionName} for changes in document {documentId}", 
            collection.CollectionNamespace.CollectionName,
            documentId
        );

        var changeStream = await collection.WatchAsync(cancellationToken: cancellationToken);
        while (await changeStream.MoveNextAsync(cancellationToken: cancellationToken))
        {
            foreach (var change in changeStream.Current)
            {
                logger.LogDebug("Collection change detected: {0}", change.FullDocument);
                if (change.FullDocument.Id == documentId)
                {
                    logger.LogInformation("Document `{documentId}` changed - pushing data to configuration store", documentId);
                    store.SetAll(change.FullDocument.Value.ToConfigurationDictionary());
                }
            }
        }
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoBackgroundConfigurationLoader(this IServiceCollection services, string documentId)
    {
        return services.AddSingleton<IHostedService>(sp => {
            var collection = sp.GetRequiredService<IMongoCollection<ConfigRecord>>();
            var factory = sp.GetRequiredService<ConfigurationBackgroundStore.Factory>();
            var logger = sp.GetRequiredService<ILogger<MongoConfigurationLoader>>();

            return new MongoConfigurationLoader(collection, factory, documentId, logger);
        });
    }
}