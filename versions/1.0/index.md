- [ ] `AddJson`
- [ ] `AddConfiNode`
- [ ] `AddPeriodic`

`Consumer`:

```csharp
builder.AddConfi();

builder.AddConfi("ConnectionStrings:Confi");

builder.AddConfi(
    baseUrl: (sp) => sp.GetRequiredService<IConfiguration>().GetRequiredValue("ConnectionStrings:Confi"),
    nodeVersion: (sp) => sp.GetRequiredService<VersionProvider>().Get(),
    nodeId: Guid.NewGuid(),
    schema: "confi.schema.json"
);

record ConfiConfiguration(
    string BaseUrl,
    string SchemaFilePath
    string AppId,
    TimeSpan RedeclareInterval,
    TimeSpan TimeToLiveForStale // by default = RedeclareInterval * 3
);

// builder.Configuration.AddJson("ConnectionStrings:Confi") 
// builder.Services.AddConfiNode(nodeVersion, nodeId, nodeSchema)
// builder.Services.AddPeriodic<ConfiNode>(s => s.SelfDeclare(), TimeSpan.FromSeconds(2)); 
```

`SelfDelcaration`: 

```ruby
schema = fromFile _schemaFilePath
    
http.put
    url = baseUrl + "/nodes"
    body =
        appId = _connectionString.appId
        version = _version
        nodeId = _nodeId
        schema = schema
        configuration = _configuration.ToJson(schema)
```