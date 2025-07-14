- [ ] Host Improved Node API
    - [ ] Node Version Returned
    - [ ] `Syncing` Status Support
        - [ ] `Configuration.LastUpdateTime` support
        - [ ] Status = `SYNCING` if `configuration.lastUpdateTime > node.lastUpdateTime`
- [ ] Consumer Node MVP
    - [ ] Flexible Configuration. [Details](#consumer-flexible-configuration)
    - [ ] Node Sync Service

## Consumer Flexible Configuration

List Of Configurable Things:

**Connection-Related**:
- `BaseUrl` (Schema, Host, Port) - have default value, but environment specific
- `Auth` - optional, environment specific
- `AppId` - required ❗. Potentially static for an app, therefore can be hard-coded

**Declaration-Related:**
- `NodeId` - can be generated at startup as guid or provided from orchestrator like K8S
- `Version` - semi-static for app, can be soft-coded or passed at built-time.
- `Schema` - static for an app, therefore can be hard-coded, but presumably in a predefined file (`confi.schema.json`). 
Shouldn't be from settings environment-dependent settings

```csharp
public class ConfiSettings
{
    public string BaseUrl { get; set;}

    [Required("AppId is Required, but wasn't provided. Recommended way to set it is via parameter is Confi Connection String")]
    public string AppId { get; set; } = null!;

    // Optional, connection string based parameter in the future:
    // public string Auth { get; set; }

    public string NodeId { get; set; } = Guid.CreateVersion7().ToString(); // can be overrided by Confi:NodeId and Hostname
    public string Version { get; set; } = "Unknown"; // overriden from Version or from VersionProvider

    [Required(ErrorMessage = "Schema is required. Ensure the `confi.schema.json` file is present or configure the schema directly")]
    public JsonSchema Schema { get; set; }
}

public static OptionsBuilder<ConfiSettings> AddConfi(this IApplicationBuilder builder, string? url)
{
    url ?= builder.Configuration("Confi:Url");
    var parsedUrl = ConfiConnectionString.Parse(url); // throws if appId is not present

    builder.Services
        .AddOptions<ConfiSettings>(parsedUrl.appId)
        .Configure((options) => {
            options.BasedUrl = parsedUrl.BaseUrl;
            options.AppId = parsedUrl.AppId;
            options.NodeId = parsedUrl.NodeId; // may not be set
        })
        .PostConfigure((options, sp) => {
            if (options.NodeId = null)
                options.NodeId = builder.Configuration["Confi:NodeId"]
                    ?? builder.Configuration["Hostname"]
                    ?? Guid.CreateVersion7();
        })
        .PostConfigure((options, sp) => {
            if (options.AppVersion = null)
                options.AppVersion = builder.Configuration["Confi:AppVersion"]
                    ?? builder.Configuration["app"]
                    ?? sp.GetService<VersionProvider>()?.Get()
                    ?? "unspecified";
        })
        // Can be assigned from UseSchemaFile(string fileName)
        .PostConfigure(options => {
            if (options.Schema == null && File.Exists("confi.schema.json"))
                options.Schema = JsonSchema.FromFile("confi.schema.json")
        });
}
```

**Drawbacks:**

- No on-the-fly updates for confi settings.