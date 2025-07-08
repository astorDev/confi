using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Confi;

public class HttpStreamPoller(IHttpClientFactory httpClientFactory, Uri endpoint, TimeSpan refreshInterval) : ConfigurationPoller(refreshInterval)
{
    public override async Task<IDictionary<string, string?>> Get()
    {
        var client = httpClientFactory.CreateClient();
        await using var stream = await client.GetStreamAsync(endpoint);
        return JsonConfigurationStreamParser.Parse(stream);
    }
}

public static class JsonHttpConfiguration
{
    public class EventsDispatcher(HttpStreamPoller poller)
    {
        public void AddErrorListener(Action<Exception> listener)
        {
            poller.AddListener(listener);
        }

        public void AddLoadedListener(Action<IDictionary<string, string?>> listener)
        {
            poller.AddListener(listener);
        }

        public void RegisterLogging(ILogger logger)
        {
            AddErrorListener(ex => logger.LogError(ex, "Error fetching JSON configuration"));
            AddLoadedListener(config => logger.LogDebug("Configuration loaded"));
        }

        public void RegisterLogging(IServiceProvider serviceProvider)
        {
            RegisterLogging(serviceProvider.GetRequiredService<ILogger<Source>>());
        }
    }

    public class Source : IConfigurationSource
    {
        private HttpClientFactory clientFactory;
        private HttpStreamPoller poller;
        private PolledConfigurationProvider provider;

        public EventsDispatcher EventsDispatcher => new(poller);

        public Source(Uri uri, TimeSpan refreshInterval)
        {
            clientFactory = new HttpClientFactory();
            poller = new HttpStreamPoller(clientFactory, uri, refreshInterval);
            provider = new PolledConfigurationProvider(poller);
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            poller.PollSync();
            return provider;
        }
    }
}

public static partial class Registration
{
    public static JsonHttpConfiguration.EventsDispatcher AddJsonHttp(this IConfigurationBuilder builder, string uri, TimeSpan? refreshInterval = null)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) throw new ArgumentException("Invalid URI format.", nameof(uri));

        return builder.AddJsonHttp(parsedUri, refreshInterval);
    }

    public static JsonHttpConfiguration.EventsDispatcher AddJsonHttp(this IConfigurationBuilder builder, Uri uri, TimeSpan? refreshInterval = null)
    {
        var source = new JsonHttpConfiguration.Source(uri, refreshInterval ?? TimeSpan.FromSeconds(5));
        builder.Add(source);
        return source.EventsDispatcher;
    }
}