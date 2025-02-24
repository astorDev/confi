using Confi.Manager;
using Nist;

namespace Confi;

public static class Entrypoint
{
    public static IEndpointRouteBuilder MapConfiManager(this IEndpointRouteBuilder endpoints) 
    {
        endpoints.MapNodes();

        return endpoints;
    }

    public static Error? ToConfiManagerError(this Exception exception)
    {
        return NodeEntrypoints.MapNodesErrors(exception) ?? null;
    }
}