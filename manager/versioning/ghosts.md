## Scenario 2: Field Rename

**Initial:**

```json
{
    "name" : "Egor Tarasov"
}
```

The user decides to change the setting name. In theory on start

**Updated:**

```json
{
    "fullName" : "Egor Tarasov"
}
```

Let's say we use "always-forward" approach with "ghosts". 

- We automark a received schema, along with a node
- We extend, but mark fields, that are not present in current schema as ghost (with json comments)
- This way we will get the following database
- Additional: If we enable node tracking we can auto-mark ghosts in "Purgatory" (still used). "Purgatory Watcher"
- We can add various ghost clean up buttons:
    - "Clean Up Ghosts" when we don't do node tracking
    - "Clean Up Free-To-Go Ghost" 

```json
{
    "name" : "Egor Tarasov", // ghost from v1 | free to go
    "firstAndLastName" : "Egor Tarasov", // ghost from v2 | in purgatory by nodes: yyy
    "fullName" : "Egor Tarasov"
}
```

```json
{
    "id" : "my-app-x",
    "currentSchema" : "",

    "nodes" : {
        "xxx" : {
            "version" : "v1",
            "status" : "active"
        }
    }
}
```