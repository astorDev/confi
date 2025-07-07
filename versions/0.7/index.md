- [x] `Confi.Manager.UI`
    - [x] Configuration Editing Display
- [x] `Confi.Manager`
    - [x] Mock Demo Configurations
    - [x] Apps List Endpoints. [Details](#apps-list-endpoints)

## App List Endpoints

Implement 2 new endpoints for working with apps.

- Put endpoint contracts into `protocol` project in a dedicated file in accordance with standard structure there.
- Put the endpoints itself along with their logic into project `endpoints`
- If a certain logic required for working with database try to make it a reusable, generic Mongo extension.

```ruby
get 'apps'
    appIds = mongo.schemas.select => @.id
    appInfos = mongo.apps.all

    return appIds.select =>
        id = appId
        name = appInfos[appIds]?.name

patch `apps/{id}`
    mongo.app.update
        by = @.id = @id
        update = 
            name = @changes.name
        insertIfUnpresent = true


    return mongo.app.getById @id
```