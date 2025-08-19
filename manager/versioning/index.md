# Versioning

Confi requires schema versioning for smooth migration. For example, if a field is removed in a newer version an app using an old version should still be able to get the field value. To achieve that, an app will pull only configuration, belonging to it's version. That implies that an app knows it's own schema version. 

There are multiple possible options for schema version number to consider:

- Version In Schema File.
- Version Assigned by Manager.

If you want to understand why - consult the [dismissed]() document.

## Version In Schema File

**Drawbacks:**

- It is likely that developer will forget to update the version
    - It will raise an error from confi manager, but this will be annoyting

## Version Assigned by Manager

Manager provides an endpoint for declaration. If the accepted schema does not match **latest** (current) schema - a new schema version record is created.

> Note: If a manager's schema matches an old schema it still considered new. This handles scenario where field semantic has changed, therefore a property was deleted and then reinstantiated with a new value.

**Benefits:**

- Fully automatic

**Drawbacks:**

- Additional complexity in manager
- Requires opinionated versioning
- Doesn't allow a version upgrades without schema changes 
    - Those version upgrades may be useful for observability

## Using App Version

We use app version, which is updated more frequently then the schema. But the schema 

**Benefits:**

- Flexibility, with possibility for enhanced transparency
- Automatic versioning

**Drawbacks:**

- Requires Version Management from Consumer.
- Unnecessary load on manager.

## Hybrid: Manager & App Version

If app version was provided in one of the ways:

- `Version` configuration variable (`VERSION` environment variable)
- `Confi:Version` configuration variable
- Supplied directly to configuration method

Then consumer uses the version explicitly

**Drawbacks:**

- May require branching in consumer logic.
- Risk of invalid sorting (the newer will become not greater)
    - Possible work-around: 
        - Use `creationTime` instead of version number for sorting.
            - This way we can not "insert" a previous version
                - Which is hardly a good idea anyway.
        - Prohibit self-assinged versioning if provided explicitly before
            - Drawbacks: This will lock consumer
        - Use `+0001` schema, which will be outbidden by virtually any self-assigned version.

```http
PUT /apps/thor/versions/latest

{
    "schema": {
        "type": "object",
        "properties": {
            "nickname": {
                "type": "string"
            }
        },
        "required": [
            "nickname"
        ]
    },
    "configuration": {
        "nickname": "Thor"
    }
}
```