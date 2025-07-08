using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confi;

public static class JsonHttpConfiguration
{
    public class Source(Uri uri) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var services = new ServiceCollection();

            services.AddHttpClient("main", cl => { cl.BaseAddress = uri; });
            services.AddSingleton<HttpGetListenable>();

            var serviceProvider = services.BuildServiceProvider();
            var listenable = serviceProvider.GetRequiredService<HttpGetListenable>();

            listenable.Start();

            return new Provider(listenable);
        }
    }

    public class Provider : ConfigurationProvider
    {
        public Provider(Listenable<Stream> listenable)
        {
            listenable.AddListener(stream =>
            {
                Data = JsonConfigurationStreamParser.Parse(stream);
                OnReload();
            });
        }
    }
}

public static partial class Registration
{
    public static IConfigurationBuilder AddJsonHttp(this IConfigurationBuilder builder, string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
        {
            throw new ArgumentException("Invalid URI format.", nameof(uri));
        }

        return builder.Add(new JsonHttpConfiguration.Source(parsedUri));
    }

    public static IConfigurationBuilder AddJsonHttp(this IConfigurationBuilder builder, Uri uri)
    {
        return builder.Add(new JsonHttpConfiguration.Source(uri));
    }
}

public class HttpGetListenable(IHttpClientFactory factory) : Listenable<Stream>
{
    public void Start()
    {
        _ = Go();
    }

    public async Task Go()
    {
        try
        {
            var stream = await GetStreamAsync();
            Notify(stream);
        }
        catch (Exception ex)
        {
            // Handle exceptions, e.g., log them
            Console.WriteLine($"Error starting HttpGetListenable: {ex.Message}");
        }
    }

    public async Task<Stream> GetStreamAsync()
    {
        var client = factory.CreateClient("main");
        var response = await client.GetAsync("");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }
}

public class Listenable<T>
{
    private readonly List<Action<T>> _listeners = new();

    public void AddListener(Action<T> listener)
    {
        _listeners.Add(listener);
    }

    public void Notify(T value)
    {
        foreach (var listener in _listeners)
        {
            listener(value);
        }
    }
}

public static class TaskExtensions
{
    public static Task OnSuccess<T>(this Task<T> task, Action<T> action)
    {
        return task.ContinueWith(t =>
        {
            action(t.Result);
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }
}