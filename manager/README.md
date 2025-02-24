## Confi Manager

Confi Manager allows distributed configuration editing and synchronization. The manager accepts the JSON scheme of the configuration from nodes and provides an editor for an admin. When the configuration is changed it updates the value. Nodes periodically read the current configuration and update it. Nodes also send their full current configuration to the Manager. The manager compares the current configuration of a node with the target configuration and determines the node's status, shown in the UI.

Confi Manager is .NET [NIST](https://github.com/astorDev/nist) API.

## Running the App 🚀

You can run the app directly:

```sh
cd host && dotnet run
```

Or via docker

```sh
docker compose up -d
```

## Testing 🧪

The app can be tested via .NET tests:

```sh
cd tests && dotnet test
```

Or via [httpyac CLI](https://httpyac.github.io/guide/installation_cli). 

> This tests are run against a running app instance, so don't forget to run it first

```sh
cd tests && httpyac send --all *.http --env=local
```