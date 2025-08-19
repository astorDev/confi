using MongoDB.Driver;
using Persic;

namespace Confi.Manager;

public static class AppListEndpoints
{
    public static IEndpointRouteBuilder MapAppListEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // endpoints.MapGet(Uris.Apps, GetApps);
        // endpoints.MapPatch(Uris.App("{appId}"), PatchApp);
        return endpoints;
    }

    // public static async Task<AppCollection> GetApps(
    //     IMongoCollection<SchemeRecord> schemasCollection,
    //     IMongoCollection<AppRecord> appCollection
    // )
    // {
    //     var schemaIds    = await schemasCollection.ToArray(x => x.Id);
    //     var appNamesDict = await appCollection.ToDictionary(x => x.Id, x => x.Name);

    //     var items = schemaIds.Select(id =>
    //         new AppSummary(
    //             Id: id,
    //             Name: appNamesDict.GetValueOrDefault(id, id)
    //         )
    //     ).ToArray();

    //     return new AppCollection(
    //         Count: items.Length,
    //         Items: items
    //     );
    // }

    // public static async Task<AppSummary> PatchApp(
    //     string appId,
    //     AppChanges changes,
    //     IMongoCollection<SchemeRecord> schemasCollection,
    //     IMongoCollection<AppRecord> appCollection
    // )
    // {
    //     var name = changes.Name ?? appId;
    //     await appCollection.Put(appId, new UpdateDefinitionBuilder<AppRecord>()
    //         .Set(x => x.Name, name)
    //     );

    //     return new AppSummary(
    //         Id: appId,
    //         Name: name
    //     );
    // }
}

public record AppRecord(string Id, string Name) : IMongoRecord<string>;

public record AppCollection(
    int Count,
    AppSummary[] Items
);

public record AppChanges(
    string? Name
);
