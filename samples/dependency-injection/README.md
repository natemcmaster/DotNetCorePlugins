Dependency Injection Sample
===========================

This sample contains 4 projects which demonstrate a plugin scenario which coordinates types between
plugins using a dependency injection container.

* 'DI.HostApp' is a console application which scans for a 'plugins' folder in its base directory and attempts to load any plugins it finds. It then configures plugins in a dependency injection collection.
* 'DI.SharedAbstractions' which contains an interface shared by plugins and the host.
* 'MyPlugin1' and 'MyPlugin2' implement shared abstractions and register them with the host.

## Running the sample

Open a command line to this folder and run:

```
dotnet restore
dotnet run -p DI.HostApp/
```
