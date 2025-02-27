using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Confi;

public class MongoBackgroundConfigurationWatcher(MongoConfigurationLoader loader) : BackgroundService
{
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
                loader.Logger.LogError(ex, "Error while watching for changes");
                await Task.Delay(500, stoppingToken);
            }
        }
    }

    private async Task LoadInitialConfigurationAsync(CancellationToken cancellationToken)
    {
        loader.Logger.LogInformation("Loading initial configuration from collection {collectionName} for document {documentId}", 
            loader.CollectionName,
            loader.DocumentId
        );

        var document = await loader.SearchAsync(cancellationToken);
        if (document is not null)
        {
            loader.Upload(document);
        }
    }

    private async Task RunWatchingAsync(CancellationToken cancellationToken)
    {
        loader.Logger.LogInformation("Starting watching collection {collectionName} for changes in document {documentId}", 
            loader.CollectionName,
            loader.DocumentId
        );

        var changeStream = await loader.Collection.WatchAsync(cancellationToken: cancellationToken);
        while (await changeStream.MoveNextAsync(cancellationToken: cancellationToken))
        {
            foreach (var change in changeStream.Current)
            {
                loader.Logger.LogDebug("Collection change detected: {0}", change.FullDocument);
                if (change.FullDocument.Id == loader.DocumentId)
                {
                    loader.Logger.LogInformation("Document `{documentId}` changed - pushing data to configuration store", loader.DocumentId);
                    loader.Upload(change.FullDocument);
                }
            }
        }
    }
}