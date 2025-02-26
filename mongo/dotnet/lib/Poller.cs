using Backi.Timers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Confi;

public class MongoConfigurationPoller(MongoConfigurationLoader loader) : IHostedService
{
    SafeTimer _timer = null!;
    Dictionary<string, object>? known;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        loader.Logger.LogInformation("Starting Polling configuration from collection {collectionName} for document {documentId}", 
            loader.CollectionName,
            loader.DocumentId
        );
        
        _timer = SafeTimer.RunNowAndPeriodically(
            TimeSpan.FromSeconds(2),
            async () => await PollConfiguration(cancellationToken: cancellationToken),
            ex => loader.Logger.LogError(ex, "Error while polling configuration")
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        loader.Logger.LogInformation("Stopping poller timer");
        
        _timer.Stop();

        return Task.CompletedTask;
    }
    
    private async Task PollConfiguration(CancellationToken cancellationToken)
    {
        loader.Logger.LogDebug("Polling configuration from collection {collectionName} for document {documentId}", 
            loader.CollectionName,
            loader.DocumentId
        );

        var document = await loader.SearchAsync(cancellationToken);
        if (document is not null)
        {
            var current = document.ToConfigurationDictionary();
            if (known == null || !ConfigurationData.IsIdentical(known, current))
            {
                loader.Logger.LogInformation("New configuration found from polling collection {collectionName} for document {documentId}", 
                    loader.CollectionName,
                    loader.DocumentId
                );
                
                known = current;
                loader.Upload(document);
            }
        }
    }
}
