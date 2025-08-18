- [ ] Self-Declaring Consumer. [Details](#self-declaring-consumer)
- [ ] UI Configuration Value Editing with Schema Validation. [Details](#ui)

## Self-Declaring Consumer

**In-Scope:**

- [migrations](migrations.md)

**Prototype:**

```ruby
addConfi
    client = confiClientFrom @connectionSettings

    client.putAppVersion
        @version # if app version already exists - returns it
        @schema # if app not exists, creates it and sets schema as passed
        value = @iconfiguration.asJsonFrom @schema
        mergePreference = 'preferPrevious' # Overrides passed value by old values by default (alt: `preferPassed`)

    listenables = @iconfiguration.addJsonHttp 
        @connectionSettings.toConfigurationUrl @version

    addLoggingBackgroundService @services listenables
```

## UI

**Out-of-scope:**

- Nodes Display
- Non-latest Versions