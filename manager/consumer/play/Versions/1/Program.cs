using Confi;
using Confi.Manager.Consumer;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddFluentEnvironmentVariables();

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddConfiConsumerNode("playground", JsonSchema.FromFile("confi-schema.json"));

var app = builder.Build();

await app.Services.ExecuteInScope<ConsumerNode>(n => n.SelfDeclare());

app.MapGet("/", async (Confi.Manager.Client managerClient) =>
{
    return await managerClient.GetApp("playground");
});

app.Run();