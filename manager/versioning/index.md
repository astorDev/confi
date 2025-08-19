---
status: being_implemented
---

# Versioning

The best way to handle breaking changes is to not make the breaking changes. In case of configuration, this roughly means to not delete outdated configuration fields. Moreover, for a simple deployment, where all the app instances updated to the newer version this should not be very annoying, since you'll need to keep those outdated fields **only** from the previously version. Once all of the deployed instances are using the latest version you should be able to safely remove the old field. 

The approach above should fit the majority of apps. However, if you want a more robust approach, we provide a way to handle this.

## Using Versions

When it makes sense:

1. You maintain multiple working app versions.
1. You don't want to care about breaking changes, even in the closest app versions.
1. You want to see old configuration schema and value versions for analytics purposes. (Without relying on Git)

For a stable work version should be under consumer's control. If you want to understand why - consult the [dismissed](dismissed.md) document. So to enable versioning, you **must** supply `version` when self-declaring your application. 


1. Value passed directly
1. Configuration value `Confi:Version`
1. Configuration value `Version`

> "Configuration value" means a configuration variable received from a source preceding Confi e.g. environment variable. Those particular variables should be embodied in something like a docker image, but we do not enforce it in any way.

## Version Conflicts

If the same `version` is used for different schemas, this is considered a mistakes because:

a. It violates the whole purpose of using Versions
b. We can imagine a circumstances, where something went wrong with version assignment and an old version was used. This may quietly break either old or new version, so an explicit error should be used instead. 

> 💡 `unspecified` Version works differently, though. It will go ahead and accept the updates, since. By the way, we hope you DO NOT have an idea to use `unspecified` as your version number. This particular value is reserved for the unversioned flow.