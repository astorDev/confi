# Confi Disabling

It may be annoying to deploy confi every time for local development. Plus. We can provide a `disabled` flag, also read from `Confi:Disabled` for disabling it for local deployments by default.

> 🤔 If a developer changes local `appsettings` in this scenario it may also be annoying to update this in remote configuration store. Versioning come in handy there, but we still want a way to allow "force" put in a convinient way. Currently there's no clear idea on how to implement that... maybe based on some indicators in a version numbers.