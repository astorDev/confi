```sh
dotnet run
```

```sh
dotnet publish && cd bin/Release/net9.0/publish && dotnet DotEnvs.Playground.dll && cd ../../../..
```

> Unlike `CopyToOutputDirectory` `CopyToPublishDirectory` does **not** make the file required.

> Don't forget to add the `.env` file to gitignore