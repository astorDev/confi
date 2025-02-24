using System.Collections;
using System.Threading.Tasks;
using MongoDB.Driver;
using Nist;

namespace Confi.Manager;

public static class AppHelper
{
    public static IEndpointRouteBuilder MapApps(this IEndpointRouteBuilder endpoints) 
    {
        endpoints.MapGet("{appId}", GetApp);

        return endpoints;
    }

    public static async Task<App> GetApp(
        string appId, 
        IMongoCollection<SchemeRecord> schemasCollection,
        IMongoCollection<NodeRecord> nodesCollection,
        IMongoCollection<ConfigurationRecord> configurationsCollection)
    {
        var schemeRecord = await schemasCollection.Search(appId) 
            ?? throw new AppNotFoundException(appId);

        var configurationRecord = await configurationsCollection.Search(appId)
            ?? throw new AppNotFoundException(appId);

        var nodeRecords = nodesCollection
            .Find(x => x.AppId == appId)
            .ToList();
        
        return new App(
            schemeRecord.Id,
            nodeRecords.ToDictionary(
                x => x.Id, 
                x => new NodeState(
                    Status: x.DetermineStatus(configurationRecord.Value)
                )
            ),
            schemeRecord.Schema.ToJsonElement(),
            configurationRecord.Value.ToJsonElement()
        );
    }

    public static Error? MapAppErrors(Exception exception)
    {
        return exception switch
        {
            AppNotFoundException e => e.ToError(),
            _ => null
        };
    }
}

public class AppNotFoundException(string appId) : Exception
{
    public override IDictionary Data => new Dictionary<string, string> {
        { "appId", appId }
    };

    public Error ToError() => Errors.AppNotFound;
}