# Configuration Migration (Schema Updates)

We need to handle various scenarios when configuration schema changes over time. We need to ensure graceful migration. Additionally, we need to assume that `name` was changed from confi UI and does not match value in `appsettings.json` for example

> In all scenarios we assume second node is updated with some delay, so it keeps sending old schema and accept old configuration. (Hence we needs to be handled gracefully)

## Scenario 1: Simple extension

**Initial:**

```json
{
    "name" : "Egor Tarasov"
}
```

**Updated:**

```json
{
    "name" : "Egor Tarasov",
    "age" : 30
}
```

## Scenario 2: Field Rename

**Initial:**

```json
{
    "name" : "Egor Tarasov"
}
```

**Updated:**

```json
{
    "fullName" : "Egor Tarasov"
}
```

## Scenario 3: Field Semantic Change

> Probably not recommended anyway

**Initial:**

```json
{
    "name" : "Egor Tarasov"
}
```

**Second:**

```json
{
    "fullName" : "Egor Tarasov"
}
```

**Finale:**

```json
{
    "name" : "Egor",
    "lastName" : "Tarasov",
    "fullName" : "Egor Tarasov"
}
```

## Scenario 4: Removed

**Initial:**

```json
{
    "name" : "Egor Tarasov",
    "age" : 30
}
```

**Final:**

```json
{
    // name is now set via local config sources (json, env vars, etc.)
    "age" : 30
}
```

## Scenario 5

1. Node 1 uploads with `nickname = Thor`
2. Value Changed in Confi to `nickname = Tornado` (Node 1 value updates)
3. Node 2 uploads with `nickname = Thor`. 
    - The value is not updated since new schema version was not provided.
4. Node 2 reads current configuration with `nickname = Tornado`
    - `nickname = Tornado` takes precedence and configuration is done using it


## Scenario 6

1. Node 1 uploads with `nickname = Thor`
2. Value Changed in Confi to `nickname = Tornado` (Node 1 value updates)
3. Node 1 uploads with `nickname = Thor` (and a newer verion number)
    - The value is not updated since new schema version was not provided.
4. Node 2 reads current configuration with `nickname = Tornado`
    - `nickname = Tornado` takes precedence and configuration is done using it

## Option 1: Brute-force 

We always just append any new field and send a combined structure - sort of an appending optimistic merge:

```json
{
    "name" : "Egor Tarasov",
    "age" : 31,
    "fullName" : "Egor Tarasov"
}
```

We consider node saved if all values sent by node match values in our schema - a missing node. We preserve value that was present here before.

**Issues:**

- Since it does not ever delete a field the configuration for a long-running project will be huge!

**Scenarios:**

- **Scenario 1**: ✅ - will work
- **Scenario 2**: ✅ / ⚠️ - works, but with a few problems: 
    - outdated fields even after they are not used anywhere
    - Considers old node to be synced (since all values)
- **Scenario 3**: ❌ - will use `Egor Tarasov` for `name` since this is an old field.
- **Scenario 4**: ❌ - will not remove `Egor Tarasov` for `name`.

## Option 2: All Versions Preserved

On the start confi reads `configuration/latest` to get existing fields value. Then it will supply current configuration version, based on the values read before and new schema. (If a value read before is not present in current schema it will just skip the value). In the future it will read only configuration by it's own version number.

> **Possible Alternative**: Instead of reading `latest` straight in `IConfiguration` we may "merge" it with the initially uploaded value. This will solve 

**Benefits:**

- Seems rather straightforward
- Provides schema (and config) versioning out-of-the-box
- Can edit old configuration

**Drawbacks:**

- Will use different endpoints for polling, increasing complexity.
- Requires providing a node version. 

**0.0.0**

```json
{
    "name" : "Egor"
}
```

**0.0.1**

```json
{
    "fullName" : "Egor Tarasov"
}
```

**0.0.2**

```json
{
    "name" : "Egor",
    "lastName" : "Tarasov",
    "fullName" : "Egor Tarasov"
}
```

**Scenarios:**

- **Scenario 1**: ✅ - works fine
- **Scenario 2**: ✅ - works fine
- **Scenario 3**: ✅ - will work **even if some nodes are still on `0.0.0`**, since it will read **0.0.1** on start
- **Scenario 4**: ❓ - will read the `name` on start, but then will not, so will probably return to the right value.

## Option 3: Phantom

Until all nodes are in `synced` node we return current configuration in `phantom` mode (appending merge). Once all nodes are `synced` we do clean up.

**phantom** - returned from `GET /configuration` until all nodes are synced. Removed afterwards 

```json
{
    "name" : "Egor Tarasov", // phantom field - will be removed once all nodes are synced
    "fullName" : "Egor Tarasov"
}
```

**expected** - this is what confi expects from node state to consider the node as sync.

```json
{
    "fullName" : "Egor Tarasov"
}
```

**Drawbacks:**

- Introduces a new concept of  `phantom`, hence increasing complexity.

**Scenarios:**

- **Scenario 1**: ✅ - works fine
- **Scenario 2**: ✅ - works fine
- **Scenario 3**: ✅ - will work if phantom death will be awaited
- **Scenario 4**: ❓ - will read the `name` on start, but will not, after the phantom death, so will probably return to the right value.

