using Confi.Manager;
using Nist;
using Persic;

namespace Confi;

public static class MainHelper
{
    public static IEndpointRouteBuilder MapConfiManager(this IEndpointRouteBuilder endpoints) 
    {
        endpoints.MapNodes();
        endpoints.MapApps();
        endpoints.MapConfiguration();
        endpoints.MapAppVersionEndpoints();

        return endpoints;
    }

    public static MongoRegistrationBuilder AddConfiManagerCollections(this MongoRegistrationBuilder builder)
    {
        return builder
            .AddNodeCollection()
            .AddCollection<SchemeRecord>("schemas")
            .AddCollection<ConfigurationRecord>("configs")
            .AddCollection<AppRecord>("apps")
            .AddCollection<AppVersionRecord>("appVersions");
    }

    public static Error? ToConfiManagerError(this Exception exception)
    {
        return NodeHelper.MapNodesErrors(exception)
            ?? AppEndpoints.MapAppErrors(exception)
            ?? AppVersionEndpoints.MapAppVersionErrors(exception)
            ?? null;
    }
}