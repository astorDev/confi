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

        return endpoints;
    }

    public static MongoDependencyInjection.CollectionBuilder AddConfiManagerCollections(this MongoDependencyInjection.CollectionBuilder builder)
    {
        return builder
            .AddCollection<NodeRecord>("nodes")
            .AddCollection<SchemeRecord>("schemas")
            .AddCollection<ConfigurationRecord>("configs");
    }

    public static Error? ToConfiManagerError(this Exception exception)
    {
        return NodeHelper.MapNodesErrors(exception)
            ?? AppEndpoints.MapAppErrors(exception)
            ?? null;
    }
}