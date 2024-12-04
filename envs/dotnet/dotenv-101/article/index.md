```sh
dotnet new web --name DotEnvs.Playground
```

`cd DotEnvs.Playground`

```sh
dotnet publish && cd bin/Release/net9.0/publish && dotnet DotEnvs.Playground.dll && cd ../../../..
```