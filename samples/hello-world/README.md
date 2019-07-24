"Hello World" Sample
====================

This sample contains 2 projects which demonstrate a simple plugin scenario.

1. 'HostApp' is a console application which scans for a 'plugins' folder in its base directory and attempts to load any plugins it finds
2. 'MyPlugin' which implements MVC controllers.

Normally, an ASP.NET Core MVC application must have a direct dependency on any assemblies
which provide controllers. However, as this sample demonstrates, an MVC application
can load controllers from a list of assemblies which is not known ahead of time when the
host app is built.

## Running the sample

Open a command line to this folder and run:

```
dotnet restore
dotnet run -p HostApp/
```
