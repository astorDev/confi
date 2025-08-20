# Confi - Distributed Configuration Management


- Json Configuration using Json Schema.
- Self-Declaration 
- 

Confi Manager allows distributed configuration editing and synchronization. The manager accepts the JSON scheme of the configuration from nodes and provides an editor for an admin. When the configuration is changed it updates the value. Nodes periodically read the current configuration and update it. Nodes also send their full current configuration to the Manager. The manager compares the current configuration of a node with the target configuration and determines the node's status, shown in the UI.

## Jump Start

1. Deploy a local instance of Confi. 

The most straightforward way to do is using docker compose. Here's an example `compose.yml`

```yml

```

2. 


Confi encourages self-declaration by default.

```sh
curl -X PUT http://localhost:40398/apps/confi-play/versions/unversioned/configuration \
-H "Content-Type: application/json" \
-d '{ "Logging": { "LogLevel": {
    "Confi.JsonHttpConfiguration.Source": "Debug"
  }}}'
```

```sh
curl -X PUT http://localhost:40398/apps/confi-play/versions/unversioned/configuration \
-H "Content-Type: application/json" \
-d '{ "Logging": { "LogLevel": {
    "Confi.JsonHttpConfiguration.Source": "Information"
  }}}'
```

---

Confi Manager is .NET [NIST](https://github.com/astorDev/nist) API.