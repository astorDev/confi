using Backi.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

namespace Confi;

public record ConfigRecord(string Id, BsonDocument Value) : IMongoRecord<string>;

public class MongoConfigurationPoller(
    IMongoCollection<ConfigRecord> collection, 
    ConfigurationBackgroundStore.Factory factory,
    string documentId,
    ILogger<MongoConfigurationLoader> logger) : IHostedService
{
    private readonly ConfigurationBackgroundStore store = factory.GetStore(MongoConfigurationLoader.Key);
    SafeTimer _timer;
    Dictionary<string, object>? known;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Polling configuration from collection {collectionName} for document {documentId}", 
            collection.CollectionNamespace.CollectionName,
            documentId
        );
        
        _timer = SafeTimer.RunNowAndPeriodically(
            TimeSpan.FromSeconds(2),
            async () => await PollConfiguration(cancellationToken: cancellationToken),
            ex => logger.LogError(ex, "Error while polling configuration")
        );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping poller timer");
        
        _timer.Stop();
    }
    
    private async Task PollConfiguration(CancellationToken cancellationToken)
    {
        logger.LogDebug("Polling configuration from collection {collectionName} for document {documentId}", 
            collection.CollectionNamespace.CollectionName,
            documentId
        );

        var document = await collection.Find(x => x.Id == documentId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (document is not null)
        {
            var current = document.Value.ToConfigurationDictionary();
            if (known == null || !Identical(known, current))
            {
                logger.LogInformation("New configuration found from polling collection {collectionName} for document {documentId}", 
                    collection.CollectionNamespace.CollectionName,
                    documentId
                );
                
                known = current;
                store.SetAll(document.Value.ToConfigurationDictionary());
            }
        }
    }

    private static bool Identical(Dictionary<string, object> left, Dictionary<string, object> right)
    {
        foreach (var pairFromLeft in left)
        {
            if (!right.TryGetValue(pairFromLeft.Key, out var valueFromRight))
            {
                return false;
            }

            if (!pairFromLeft.Value.Equals(valueFromRight))
            {
                return false;
            }
        }
        
        foreach (var pairFromRight in right)
        {
            if (!left.TryGetValue(pairFromRight.Key, out var valueFromLeft))
            {
                return false;
            }

            if (!pairFromRight.Value.Equals(valueFromLeft))
            {
                return false;
            }
        }
        
        return true;
    }
}


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
    public static IServiceCollection AddMongoBackgroundConfigurationLoader(this IServiceCollection services, string documentId, MongoLoadingMode? mode = MongoLoadingMode.CollectionWatch)
    {
        mode ??= MongoLoadingMode.CollectionWatch;

        if (mode == MongoLoadingMode.CollectionWatch)
        {
            return services.AddSingleton<IHostedService>(sp => {
                var collection = sp.GetRequiredService<IMongoCollection<ConfigRecord>>();
                var factory = sp.GetRequiredService<ConfigurationBackgroundStore.Factory>();
                var logger = sp.GetRequiredService<ILogger<MongoConfigurationLoader>>();

                return new MongoConfigurationLoader(collection, factory, documentId, logger);
            });    
        }
        else
        {
            return services.AddSingleton<IHostedService>(sp => {
                var collection = sp.GetRequiredService<IMongoCollection<ConfigRecord>>();
                var factory = sp.GetRequiredService<ConfigurationBackgroundStore.Factory>();
                var logger = sp.GetRequiredService<ILogger<MongoConfigurationLoader>>();

                return new MongoConfigurationPoller(collection, factory, documentId, logger);
            });  
        }
    }
}

public enum MongoLoadingMode
{
    CollectionWatch,
    LongPolling
}