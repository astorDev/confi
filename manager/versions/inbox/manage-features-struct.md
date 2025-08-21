With Cross-Cutting Features:

```
📁 manage
    📁 nodes
        # components and play? 
        # probably can be called `js` or even `shadcn` to allow `flutter`?
        # contains export "view" for nodes bar. 
        # Updating via polling?
        # How to test? In theory may use stub api endpoint. (not using real protocol)
        📁 ui
        
        # ⚠️ hard to make meaningful with circular deps for NodeCandidate for example
        # Alternatively requires `core` protocol parts package, but seems like overkill, tbh
        📁 protocol
            📄 app-node-collection.js # a dictionary essentially

        # ⚠️ Not much things to be made isolated, maybe just GetNodeId, but seems overkill
        📁 consumer
        📁 manager
```

```
📁 core
    # only core models and like JsonSchema?
    📁 protocol
    📁 

```

**Open Questions**

- How to handle related, non-core dependencies e.g. versions and nodes
    - Possible answers
        - Consider `versions` a core module
        - Nodes rely on versions.