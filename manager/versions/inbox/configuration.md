# Confi Configuration

Thoughts on how to setup flexible, yet simple configuration for confi. Here are the things to configure:

**Connection-Related**:

- `BaseUrl` (Schema, Host, Port) - have default value, but environment specific
- `Auth` - optional, environment specific
- `AppId` - required ❗. Potentially static for an app, therefore can be hard-coded

**Declaration-Related:**

- `NodeId` - can be generated at startup as guid or provided from orchestrator like K8S
- `Version` - semi-static for app, can be soft-coded or passed at built-time.
- `Schema` - static for an app, therefore can be hard-coded, but presumably in a predefined file (`confi.schema.json`). Shouldn't be from settings environment-dependent settings

## Requirements

**Nice to have:**

- Provide `AddConfiOptional`, so that it can be included in NIST template, promoting it, but not enforcing.

## Option 1: Centralized Settings & PostConfigure

**Drawbacks:**

- No on-the-fly updates for confi settings.
- Unseparable settings object

**Benefits**

- May be reconfigured both on environment variables level and on 
- Clear, centralized place for configuration, along with the clear validation technique

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
        .AddOptions<ConfiSettings>()
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

```csharp
public static void AddConfiServices(this IApplicationBuilder builder, ConfiConnectionString connectionString)
{
    builder.AddJsonHttp("");
}
```

## Option 2: Split Settings & Configure

**Drawbacks:**:

- Incomplete `ConfiSelfDeclarationSettings` since both `AppId` & `BaseUrl` actually needed for declaration.

**Tactical Alternatives:**

- Inherit `ConfiSelfDeclarationSettings` from `ConfiConnectionSettings` - unclear how to bind it with MS options system.

**Reference Code:**

```csharp
public class ConfiConnectionSettings
{
    public string BaseUrl { get; set;}

    [Required("AppId is Required, but wasn't provided. Recommended way to set it is via parameter is Confi Connection String")]
    public string AppId { get; set; } = null!;

    // Optional, connection string based parameter in the future:
    // public string Auth { get; set; }
}

public class ConfiSelfDeclarationSettings
{
    public required string NodeId { get; set; } // can be overrided by Confi:NodeId and Hostname

    public required string Version { get; set; } // overriden from Version or from VersionProvider

    [Required(ErrorMessage = "Schema is required. Ensure the `confi.schema.json` file is present or configure the schema directly")]
    public required JsonSchema Schema { get; set; }
}

public record ConfiBuilder(
    OptionsBuilder<ConfiConnectionSettings> ConnectionOptions,
    OptionsBuilder<ConfiSelfDeclarationSettings> SelfDeclarationOptions
);

public static ConfiBuilder AddConfi(this IApplicationBuilder builder, string? url)
{
    url ?= builder.Configuration("Confi:Url");
    var parsedUrl = ConfiConnectionString.Parse(url); // throws if appId is not present

    builder.Services
        .AddOptions<ConfiConnectionSettings>(parsedUrl.appId)
        .Configure((options) => {
            options.BasedUrl = parsedUrl.BaseUrl;
            options.AppId = parsedUrl.AppId;
        });

    builder.Services
        .AddOption<ConfiSelfDeclarationSettings>()
        .PostConfigure((options, sp) => {
            if (options.NodeId == null)
                options.NodeId = builder.Configuration["Confi:NodeId"]
                    ?? builder.Configuration["Hostname"]
                    ?? Guid.CreateVersion7();
        })
        .PostConfigure((options, sp) => {
            if (options.AppVersion == null)
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

```csharp
builder.Configuration.AddConfi(); // based on ConfiConnectionString. 
// Accepts the configuration in the following ways:
//
// 1. Reads connection string from "ConnectionStrings:Confi" (by default parameterless)
// 2. Raw Connection String (string argument)
// 3. ConfiConnectionSettings object

builder.Services.AddConfiSelfDeclarationSettings(); // based on ConfiSelfDeclarationSettings

app.SelfDeclareInConfi(); // accepts ConfiSelfDeclarationSettings or resolves them from DI
```

## Option 3: DI-Based Self Declaration

**Benefits:**

- Allows the most flexible configuration. Both using environment variables and custom DI overrides

**Drawbacks:**

- Unclear how to make validation nicely e.g. for confi.schema file being not present
    - Options have `ValidateOnStart`, here we will need something written by hand
- Can **not** use options `Configure` infrastructure (since options are not used in this approach)

```csharp
builder.AddConfi();

// user can implement custom services
builder.Services.AddSingleton<IConfiNodeIfProvider, CustomConfiNodeIdProvider>();

// for simple usages lambda 
builder.Services.AddConfiVersionProviderLambda(sp => sp.GetService<CustomVersionProvider>().Get())

public static ConfiBuilder AddConfi(this IApplicationBuilder builder, string? url)
{
    url ?= builder.Configuration("Confi:Url");
    var parsedUrl = ConfiConnectionString.Parse(url); // throws if appId is not present

    builder.Services
        .AddOptions<ConfiConnectionSettings>(parsedUrl.appId)
        .Configure((options) => {
            options.BasedUrl = parsedUrl.BaseUrl;
            options.AppId = parsedUrl.AppId;
        });

    builder.Services
        .AddScoped<ConfiClient>(); // based on IOptions<ConfiConnectionSettings>.
        .AddSingleton<IConfiVersionProvider, StandardConfiVersionProvider>() // Confi:NodeId -> Hostname -> Guid.CreateVersion7() 
        .AddSingleton<IConfiNodeIdProvider, StandardConfiNodeIdProvider>() // Confi:AppVersion -> VersionProvider.Get() -> "0.0-unspecified"
        .AddSingleton<IConfiSchemaProvider, StandardConfiSchemaProvider>() // JsonSchema.FromFile("confi.schema.json") - Unclear how to do validation nicely
        ;
}

public class ConfiLambdaVersionProvider(Func<string> provision) : IConfiVersionProvider
{
    Get() => provision();
}
```