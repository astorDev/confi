## Version In Schema File

**Drawbacks:**

- It is likely that developer will forget to update the version
    - It will raise an error from confi manager, but this will be annoyting

## Version Assigned by Manager

Manager provides an endpoint for declaration. If the accepted schema does not match **latest** (current) schema - a new schema version record is created.

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

Then consumer uses the version explicitly. Otherwise `latest` version is supplied in `PUT`, then the returned version used for the pull.

**Drawbacks:**

- May require branching in consumer logic.
- Risk of invalid sorting (the newer will become not greater)
    - Possible work-around: 
        - Use `creationTime` instead of version number for sorting.
            - This way we can not "insert" a previous version.
                - Which is hardly a good idea anyway.
        - Prohibit self-assinged versioning if provided explicitly before
            - Drawbacks: This will lock consumer
        - Use `+0001` schema, which will be outbidden by virtually any self-assigned version.

**Scenario:**

1. Old node (1) supplies `latest`, get assinged `2025.102.103.1`
2. New node (2) supplies `latest`, get assigned `2025.102.103.2`
3. Old node restart, supplies `latest` - get assigned `2025.102.103.3`, because it wasn't matching current latest. (❌ Problem!!)

**Possible workaround**: `latest` should compare to all the previous schema, not just the latest one. Probably can be done faster by utilizing SHA-256 or other hashes.

## Scenario: Reassigned semantic

State 1: `{ "name" : "Egor Tarasov" }`
State 2: `{ "name" : "Egor", "fullName" : "Egor Tarasov" }`
State 3: `{ "name" : "Egor" }`

Problem With Automatic Assingment: If State 3 will be compares to all other schemes, it will match the state 1 and will use 

## Is it really needed?

Let's say a Confi user doesn't want to care about versioning at all? The user will have trouble **removing** fields or changing they semantics. Incremental approach will work though.