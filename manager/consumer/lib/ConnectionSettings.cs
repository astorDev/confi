using System.Net.Http.Json;
using Confi.Manager;

namespace Confi;

public record ConfiConnectionSettings(Uri BaseUri, string appId)
{

    public static ConfiConnectionSettings Parse(string connectionString)
    {
        var connectionStringUri = new Uri(connectionString);
        var appId = connectionStringUri.Segments.ElementAtOrDefault(1) ?? throw new FormatException("Connection string must end with an appId segment");

        var baseUriString = connectionStringUri.GetLeftPart(UriPartial.Authority);
        var baseUri = new Uri(baseUriString);

        return new ConfiConnectionSettings(baseUri, appId);
    }

    public HttpClient CreateHttpClient() => new()
    {
        BaseAddress = BaseUri
    };

    public async Task PutCandidate(AppVersionCandidate candidate, string version = Uris.Unversioned)
    {
        await CreateHttpClient().PutAsJsonAsync(
            Uris.AppVersion(appId, version),
            candidate
        );
    }

    public string PullUriString(string version = Uris.Unversioned)
        => $"{BaseUri}{Uris.AppVersionConfiguration(appId, version)}";
}