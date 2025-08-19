- [ ] Self-Declaring Consumer. [Details](#self-declaring-consumer)
- [ ] UI Configuration Value Editing with Schema Validation. [Details](#ui)

## Self-Declaring Consumer

**In-Scope:**

- [migrations](migrations.md)

**Prototype:**

```ruby
addConfi
    client = confiClientFrom @connectionSettings

    putApp = client.putAppVersion
        @schema # if app not exists, creates it and sets schema as passed
        value = @iconfiguration.asJsonFrom @schema
        mergePreference = 'preferPrevious' # Overrides passed value by old values by default (alt: `preferPassed`)
        # @version # only if used, otherwise automatically assigned by manager

    listenables = @iconfiguration.addJsonHttp 
        @connectionSettings.toConfigurationUrl putApp.version

    addLoggingBackgroundService @services listenables
```

## UI

**Out-of-scope:**

- Nodes Display
- Non-latest Versions