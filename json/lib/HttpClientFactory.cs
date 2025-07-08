using Microsoft.Extensions.DependencyInjection;

public class HttpClientFactory : IHttpClientFactory
{
    private readonly IHttpClientFactory _innerFactory;

    public HttpClientFactory()
    {
        _innerFactory = new ServiceCollection()
            .AddHttpClient()
            .BuildServiceProvider()
            .GetRequiredService<IHttpClientFactory>();
    }

    public HttpClient CreateClient(string name)
    {
        return _innerFactory.CreateClient(name);
    }
}