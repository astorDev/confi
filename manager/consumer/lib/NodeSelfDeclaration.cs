global using ManagerClient = Confi.Manager.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Versy;

namespace Confi.Manager.Consumer;

public class ConsumerNode(ConsumerNode.Factory factory, JsonSchema schema, string appId, string nodeId)
{
    public record Factory(IConfiguration Configuration, ManagerClient Client, VersionProvider VersionProvider)
    {
        public ConsumerNode Create(JsonSchema schema, string appId, string nodeId)
        {
            return new ConsumerNode(this, schema, appId, nodeId);
        }
    }

    public NodeCandidate Candidate => CreateCandidate(schema, factory.Configuration, factory.VersionProvider.Get());

    public async Task<Node> SelfDeclare()
    {
        return await factory.Client.PutNode(
            appId,
            nodeId,
            Candidate
        );
    }

    public static NodeCandidate CreateCandidate(
        JsonSchema schema,
        IConfiguration configuration,
        string version
    )
    {
        var configurationJson = configuration.AsJson(schema);
        return new(
            Schema: schema.AsJsonElement(),
            Configuration: configurationJson.AsJsonElement(),
            Version: version
        );
    }
}

public static class Registration
{
    public static IServiceCollection AddConfiConsumerNode(this IServiceCollection services, string appId, JsonSchema schema, string? nodeId = null, string managerUrlPath = "ConfiManager:Url")
    {
        nodeId ??= Guid.CreateVersion7().ToString();

        services.AddVersionProvider();
        services.AddHttpService<ManagerClient>(managerUrlPath);
        services.AddScoped<ConsumerNode.Factory>();
        services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<ConsumerNode.Factory>();
            return factory.Create(schema, appId, nodeId);
        });

        return services;
    }

    // TO DO: Move to Nist.Registration ?
    public static async Task ExecuteInScope<TService>(
        this IServiceProvider services,
        Func<TService, Task> action
    )
        where TService : notnull
    {
        using var scope = services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await action(service);
    }
}