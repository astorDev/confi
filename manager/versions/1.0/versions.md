# App (Schema) Versions

Versioning is a must for ensuring [migrations](migrations.md) (if using option #2). However it brings additional overhead to the way we can organize consumer:

1. Version needs to be resolved **before** self-declaration, hence without using any service.
1. Different endpoints for initial configuration loading: (`configurations/latest`) and for subsequent sync: (`configurations/{version}`).

The simplest implementation seems to be something like that:

```ruby
addConfi
    @iconfiguration.addOneTimeHttpJson 'https://localhost/apps/my-app/configurations/latest'

    return @iconfiguration.addJsonHttp 'https://localhost/apps/my-app/configurations/{@version}'
```

However, if [we want to remove field from confi in favor of local config](migrations.md#scenario-4-removed) we should not load the field to `IConfiguration` from the `latest` json. So we should do a preliminary cleanup like this:

```ruby
addConfi
    latestConfig = http.get 'https://localhost/apps/my-app/configurations/latest'
    effectiveLatestConfig = latestConfig.reducedTo @jsonSchema
    @iconfiguration.addJsonObject effectiveLatestConfig

    return @iconfiguration.addJsonHttp 'https://localhost/apps/my-app/configurations/{@version}'
```

However, this produces the need to load `jsonSchema` before-hand.

## Automatic Versioning

Preferrably, we shouldn't require any setup from the user side to handle versioning. We can check if something is a new version by just comparing the schema, so any newly accepted schema can just get a higher version number (plus current `createTime`). This can be achieved for example by increasing plus numbers appended to the previous version `+001` i.e. if version doesn't contains a `+`, then `+001` is attached otherwise the number after `+` is increased e.g. `+042` after `+041`. Thousand iterations should be enough for the automatic increase.

```ruby
addConfi
    appState = http.get 'https://localhost/apps/my-app'
    effectiveLatestConfig = appState.configuration.reducedTo @jsonSchema
    @iconfiguration.addJsonObject effectiveLatestConfig

    version = 
        @version 
        ?? @iconfiguration["Version"]
        ?? appState.version.autoincreased

    return @iconfiguration.addJsonHttp 'https://localhost/apps/my-app/configurations/{version}'
```