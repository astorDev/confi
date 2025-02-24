using Confi.Manager;
using Nist;
using Persic;

namespace Confi;

public static class Entrypoint
{
    public static IEndpointRouteBuilder MapConfiManager(this IEndpointRouteBuilder endpoints) 
    {
        endpoints.MapNodes();
        endpoints.MapApps();

        return endpoints;
    }

    public static MongoDependencyInjection.CollectionBuilder AddConfiManagerCollections(this MongoDependencyInjection.CollectionBuilder builder)
    {
        return builder
            .AddCollection<NodeRecord>("nodes")
            .AddCollection<SchemeRecord>("schemas");
    }

    public static Error? ToConfiManagerError(this Exception exception)
    {
        return NodeEntrypoints.MapNodesErrors(exception)
            ?? AppEntrypoints.MapAppErrors(exception)
            ?? null;
    }
}