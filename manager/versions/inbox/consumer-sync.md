# Consumer Sync

- [ ] Consumer Reads Current Configuration.
- [ ] Consumer Self-Declares with an Up-To-Date Configuration.

## Sequential VS Parallel?

**Pro Parallel:**

- If node push breaks consumer will still get an up to date info
- ⭐ Can use simply (or wrapped) `AddJsonHttp` - which empowers the sublib
- Sync intervals can be configured separately
    - In sequential case we can make `PUT /nodes` run once in x (2/3) configuration reads

**Pro Sequential:**

- Normally no outdated state push
- ⭐ Allows `Syncing` status (`node.updatedAt` is before (<) `configuration.updatedAt`) - no `unsynced` status in normal flow

```csharp
var source = builder.Configuration.AddJsonHttp("http://localhost:333/my-app");

builder.Services.AddSingleton(source);

builder.Services.AddHostedService<ConfiSelfDeclarationService>();
builder.Services.AddHostedService<JsonHttpLoggingService>(); // instead of source.RegisterLogging(app.Services) - since app will not be yet available

public class ConfiSelfDeclarationService : BackgroundService
{
    public ConfiSelfDeclarationService(IOptions<ConfiSelfDeclaration> options, JsonHttpConfiguration.Source configurationSource)
    {
        // maybe should be done in the "Start".

        if (options.Value.DeclareSequentually) // default true
            configurationSource.PulledNotifier.AddListener(SelfDeclare.WithOnError(ErrorHandler));
        else
            SafeTimer.RunNowAndPeriodically("00:01", SelfDeclare, ErrorHandler);
    }

    public async Task SelfDeclare()
    {
    }
}
```