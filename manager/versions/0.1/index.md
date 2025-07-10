- [ ] Host Improved Node API
    - [ ] Node Version Returned
    - [ ] `Syncing` Status Support
        - [ ] `Configuration.LastUpdateTime` support
        - [ ] Status = `SYNCING` if `configuration.lastUpdateTime > node.lastUpdateTime`
- [ ] Consumer Node MVP
    - [ ] Flexible Configuration. [Details](#consumer-flexible-configuration)
    - [ ] Node Sync Service

## Consumer Flexible Configuration

```ruby
confiConfiguration
    Schema = "http" # https in the future
    Host = "localhost" # api.confi.live in the future
    Port = 40398 # 443 in the future
    AppId => throw 'appId was not provided'
    node => Guid.CreateVersion7
    version => @sp.GetService_VersionProvider.Get ?? "0"
    schema => JsonSchema.FromFile @path
```