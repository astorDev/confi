- [ ] Create Full Confi (Manager) Cycle
    - [ ] On Start Consumer Sends Node Info
    - [ ] Consumer Syncs with Confi Host. [Details](#consumer-sync)
    - [ ] Host Displays an Actual Info.

## Consumer Sync

- [ ] Consumer Reads Current Configuration.
- [ ] Consumer Self-Declared with an Up-To-Date Configuration

#### Is Consumer Sync a Sequential or Parallel Operation?

- Does consumer sync is one operation or smth done in parallel
    - Benefits for parallel
        - If node push breaks consumer will still get an up to date info
        - Can use simply `AddJsonHttp`
        - Sync intervals can be configured separately
            - In sequential case we can make `PUT /nodes` run once in x (2/3) configuration reads
    - Benefits for sequential
        - Normally no outdated state push
        - ⭐ Allows `Syncing` status (`node.updatedAt` is before (<) `configuration.updatedAt`) - no `unsynced` status in normal flow
