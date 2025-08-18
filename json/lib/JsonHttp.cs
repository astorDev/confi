using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Confi;

public static class JsonHttpConfiguration
{
    public class Source : IConfigurationSource
    {
        private HttpClientFactory clientFactory;
        private AsyncPoller<IDictionary<string, string?>> poller;
        private ConfigurationListener provider;

        public AsyncPoller<IDictionary<string, string?>>.ListenablesSet Listeners => poller.Listenables;

        public Source(Uri uri, TimeSpan refreshInterval)
        {
            clientFactory = new HttpClientFactory();
            poller = new AsyncPoller<IDictionary<string, string?>>(
                refreshInterval,
                async () =>
                {
                    var client = clientFactory.CreateClient();
                    await using var stream = await client.GetStreamAsync(uri);
                    return JsonConfigurationStreamParser.Parse(stream);
                },
                (a, b) => a.SequenceEqual(b)
            );

            provider = new ConfigurationListener(poller.Listenables.Updated);
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            poller.PollSync();
            return provider;
        }
    }
    
    public static AsyncPoller<IDictionary<string, string?>>.ListenablesSet AddJsonHttp(this IConfigurationBuilder builder, string uri, TimeSpan? refreshInterval = null)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) throw new ArgumentException("Invalid URI format.", nameof(uri));

        return builder.AddJsonHttp(parsedUri, refreshInterval);
    }

    public static AsyncPoller<IDictionary<string, string?>>.ListenablesSet AddJsonHttp(this IConfigurationBuilder builder, Uri uri, TimeSpan? refreshInterval = null)
    {
        var source = new Source(uri, refreshInterval ?? TimeSpan.FromSeconds(5));
        builder.Add(source);
        return source.Listeners;
    }

    public static void AddJsonHttpLoggingBackgroundService(this IServiceCollection services, AsyncPoller<IDictionary<string, string?>>.ListenablesSet listenables)
    {
        services.AddHostedService(provider => new LoggingBackgroundService(provider.GetRequiredService<ILogger<Source>>(), listenables));
    }

    public static void RegisterLogging(this AsyncPoller<IDictionary<string, string?>>.ListenablesSet listenables, ILogger logger)
    {
        listenables.Error.AddListener(ex => logger.LogError(ex, "Error fetching JSON Http configuration"));
        listenables.Loaded.AddListener(config => logger.LogDebug("Json Http Configuration Loaded"));
        listenables.Updated.AddListener(config => logger.LogInformation("Json Http Configuration Updated"));
    }

    public class LoggingBackgroundService : BackgroundService
    {
        private readonly ILogger<Source> _logger;
        private readonly AsyncPoller<IDictionary<string, string?>>.ListenablesSet _listeners;

        public LoggingBackgroundService(
            ILogger<Source> logger,
            AsyncPoller<IDictionary<string, string?>>.ListenablesSet listeners)
        {
            _logger = logger;
            _listeners = listeners;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listeners.RegisterLogging(_logger);
            return Task.CompletedTask;
        }
    }
}