# Hot Reload Sample

This is a minimal sample of how to take advantage of hot reloading support.

To run this sample, execute the `run.sh` script. This will:

* Compile the projects once
* Start the HotReloadApp console application
    * This creates a single loader with hot reloading enabled
    * It subscribes to the `PluginLoader.Reloaded` event to be notified when a new version of the assemblies
      are availabled.
    * It invokes the new version of the assembly
* Rebuilds the TimestampedPlugin every 5 seconds (until you press CTRL+C to exit)
