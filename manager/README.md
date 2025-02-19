## Confi Manager

Confi Manager allows distributed configuration editing and synchronization. The manager accepts the JSON scheme of the configuration from nodes and provides an editor for an admin. When the configuration is changed it updates the value. Nodes periodically read the current configuration and update it. Nodes also send their full current configuration to the Manager. The manager compares the current configuration of a node with the target configuration and determines the node's status, shown in the UI.
