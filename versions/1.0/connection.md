```csharp
builder.AddConfi("http://localhost:5631/thor?retry=4351&nodeVersion={nodeVersion}&nodeId={nodeId}");
```

```csharp
builder.AddConfi(
    "http://localhost:5631/thor?retryInterval=00:00:05"
    nodeId: Guid.CreateVersion7(),
    nodeVersionResolution: (sp) => sp.GetRequiredService<VersionProvider>().Get(),
    schema: JsonSchema.FromFile("confi.schema.json")
);
```

```csharp
builder.AddConfi(
    "http://localhost:5631/thor?schemaFilePath=confi.schema.json"
);
```

```csharp
builder.AddConfi(appId: "thor");
```

```csharp
builder.AddConfi("/thor", privateKeyPath: "Foonfi:PrivateKey", apiKeyPath: "Foonfi:ApiKey");
```

```csharp
builder.AddConfi(
    connectionString: ""
    // ...
)
{
    appIdResolution ??= 

}

// actual method
AddConfi(
    connectionString: "", // first parameter, optional, but can be used to resolve ALL other parameters
    schema: "http", // by default
    host: "localhost", // required
    hostResolution: () => host,
    port: 4893,
    appId: null, 
    appIdResolution: () => appId ?? throw new Exception("appId was not provided"),
    nodeId: null, // optional, by default uses just Guid.CreateVersion7
    nodeResolution: _ => Guid.CreateVersion7(),
    version: null, // can override versionResolution if needed
    versionResolution: sp => sp.GetService<VersionProvider>()?.Get() ?? "0", // can be switched, relies on optional version provider
    schemaResolution: _ => JsonSchema.FromFile(schemaFileName), //
    schemaFileName: "confi.schema.json" // can be passed from connection string
);
```