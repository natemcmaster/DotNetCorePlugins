"Hello World" Sample
====================

This sample contains 3 projects which demonstrate a simple plugin scenario.

1. 'HostApp' is a console application which scans for a 'plugins' folder in its base directory and attempts to load any plugins it finds
2. 'MyPlugin' which implements an implementation of this plugin
3. 'PluginContract' which contains an interface shared by plugins and the host.

## Running the sample

Open a command line to this folder and run:

```
dotnet restore
dotnet run -p HostApp/
```
