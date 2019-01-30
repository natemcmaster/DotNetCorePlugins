.NET Core Plugins
=================

[![Build Status](https://dev.azure.com/natemcmaster/github/_apis/build/status/DotNetCorePlugins?branchName=master)](https://dev.azure.com/natemcmaster/github/_build/latest?definitionId=6&branchName=master)

[![NuGet][main-nuget-badge]][main-nuget]
[![MyGet][main-myget-badge]][main-myget]

[main-nuget]: https://www.nuget.org/packages/McMaster.NETCore.Plugins/
[main-nuget-badge]: https://img.shields.io/nuget/v/McMaster.NETCore.Plugins.svg?style=flat-square&label=nuget
[main-myget]: https://www.myget.org/feed/natemcmaster/package/nuget/McMaster.NETCore.Plugins
[main-myget-badge]: https://img.shields.io/www.myget/natemcmaster/vpre/McMaster.NETCore.Plugins.svg?style=flat-square&label=myget

This project provides API for loading .NET Core assemblies dynamically, executing them as extensions to the main application, and finding and **isolating** the dependencies of the plugin from the main application.

Unlike other approaches to dynamic assembly loading, like `Assembly.LoadFrom`, this API attempts to imitate the behavior of `.deps.json`
and `runtimeconfig.json` files to probe for dependencies, load native (unmanaged) libraries, and to
find binaries from runtime stores or package caches. In addition, it allows for fine-grained control over
which types should be unified between the loader and the plugin, and which can remain isolated from the main
application.

Blog post introducing this project: [.NET Core Plugins: Introducing an API for loading .dll files (and their dependencies) as 'plugins'](https://natemcmaster.com/blog/2018/07/25/netcore-plugins/)

## Getting started

Pre-release builds and symbols: https://www.myget.org/gallery/natemcmaster/

You can install the plugin loading API using the `McMaster.NETCore.Plugins` NuGet package.

```
dotnet add package McMaster.NETCore.Plugins
```

The main API to use is `PluginLoader.CreateFromAssemblyFile`.

```csharp
PluginLoader.CreateFromAssemblyFile(
    assemblyFile: "./plugins/MyPlugin/MyPlugin1.dll",
    sharedTypes: new [] { typeof(IPlugin), typeof(IServiceCollection), typeof(ILogger) })
```

* assemblyFile = the file path to the main .dll of the plugin
* sharedTypes = a list of types which the loader should ensure are unified

See example projects in [samples/](./samples/) for more detailed, example usage.

## Usage

Using plugins requires at least two projects: (1) the 'host' app which loads plugins and (2) the plugin,
but typically also uses a third, (3) an abstractions project which defines the interaction between the plugin
and the host.

### The plugin abstraction

You can define your own plugin abstractions. A minimal plugin might look like this.

```csharp
public interface IPlugin
{
    string GetName();
}
```

### The plugins

Typically, it is best to implement plugins by targeting `netcoreapp2.0` or higher. They can target `netstandard2.0` as well, but using `netcoreapp2.0` is better because it reduces the number of redundant System.\* assemblies in the plugin output.

A minimal implementation of the plugin could be as simple as this.
```csharp
internal class MyPlugin1 : IPlugin
{
    public string GetName() => "My plugin v1";
}
```

### The host

The host application can load plugins using the `PluginLoader` API. The host app needs to define a way to find
the assemblies for the plugin on disk. One way to do this is to follow a convention, such as:

```
plugins/
    $PluginName1/
        $PluginName1.dll
        (additional plugin files)
    $PluginName2/
        $PluginName2.dll
```

For example, you could prepare the sample plugin above by running

```
dotnet publish MyPlugin1.csproj --output plugins/MyPlugin1/
```

An implementation of a host which finds and loads this plugin might look like this:

```csharp
using McMaster.NETCore.Plugins;

public class Program
{
    public static void Main(string[] args)
    {
        var loaders = new List<PluginLoader>();

        // create plugin loaders
        var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        foreach (var dir in Directory.GetDirectories(pluginsDir))
        {
            var dirName = Path.GetFileName(dir);
            var pluginDll = Path.Combine(dir, dirName + ".dll");
            if (File.Exists(pluginDll))
            {
                var loader = PluginLoader.CreateFromAssemblyFile(
                    pluginDll,
                    sharedTypes: new [] { typeof(IPlugin) });
                loaders.Add(loader);
            }
        }

        // Create an instance of plugin types
        foreach (var loader in loaders)
        {
            foreach (var pluginType in loader
                .LoadDefaultAssembly()
                .GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract))
            {
                // This assumes the implementation of IPlugin has a parameterless constructor
                IPlugin plugin = (IPlugin)Activator.CreateInstance(pluginType);

                Console.WriteLine($"Created plugin instance '{plugin.GetName()}'.");
            }
        }
    }
}
```

## Plugin config file

This also supports using a [config file](./docs/plugin-config.md) to control the settings of the loader per-plugin. This plugin config file can be hand-crafted, or generated using `McMaster.NETCore.Plugins.Sdk`.

```xml
<!-- A project that produces the plugin. -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.0</TargetFramework>
    <IsPlugin>true</IsPlugin>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="McMaster.NETCore.Plugins.Sdk" Version="*" />
  </ItemGroup>
</Project>
```

You can then use `PluginLoader.CreateFromConfigFile` to load the plugin from the configuration file.

```csharp
PluginLoader.CreateFromConfigFile(
    filePath: "./plugins/MyPlugin/plugin.config",
    sharedTypes: new [] { typeof(IPlugin), typeof(IServiceCollection), typeof(ILogger) })
```
