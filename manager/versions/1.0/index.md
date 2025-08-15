- [ ] Consumer Declares AppId, Schema and Value
- [ ] UI Configuration Value Editing with Schema Validation
- [ ] Consumer reads configuration by the app id

## Target

```csharp
var confiPolling = builder.Configuration.AddConfi("http://localhost:40398/my-app");
    // registers Confi:AppId and Confi:Url
    // may call AddJsonHttp("http://localhost:40398/apps/my-app");

// ...

confiPolling.RegisterLogging(app.Services);

app.SelfDeclareInConfi(); 
    // the method may accept arguments indicating from where to get the schema
    // e.g. file name or a type (for automatic schema resolution)
```

Or

```csharp
builder.AddConfi("http://localhost:40398/my-app");
    // Accepts arguments from both builder.Configuration.AddConfi and app.SelfDeclare
    // 
    // registers Confi:AppId and Confi:Url
    // 
    // registers IHostedService, that:
    // 1. Self-Declares
    // 2. Polls configuration periodically and on start
    // 3. Registers logging for poll events
    // 
    // For self declaration we both accept 
```