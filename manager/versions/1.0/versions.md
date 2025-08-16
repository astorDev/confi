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