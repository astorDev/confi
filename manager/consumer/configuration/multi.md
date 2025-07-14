```csharp
builder.AddConfi(builder.Configuration["RATES_CONFI"]).UseSchemaFile("rates.confi.schema.json");
builder.AddConfi(builder.Configuration["CURRENCIES_CONFI"]).UseSchemaFile("currencies.confi.schema.json");

// ...

await app.RunInScope<ConfiNodeFactory>(n => n.SelfDeclareAll());
```

**Requirements:**

- `appId` is accessible while configuring for using named options pattern
- 