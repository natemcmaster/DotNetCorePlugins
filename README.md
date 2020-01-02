.NET Core Plugins
=================

[![Build Status](https://dev.azure.com/natemcmaster/github/_apis/build/status/DotNetCorePlugins?branchName=master)](https://dev.azure.com/natemcmaster/github/_build/latest?definitionId=6&branchName=master)

[![NuGet][main-nuget-badge] ![NuGet Downloads][nuget-download-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/McMaster.NETCore.Plugins/
[main-nuget-badge]: https://img.shields.io/nuget/v/McMaster.NETCore.Plugins.svg?style=flat-square&label=nuget
[nuget-download-badge]: https://img.shields.io/nuget/dt/McMaster.NETCore.Plugins?style=flat-square


This project provides API for loading .NET Core assemblies dynamically, executing them as extensions to the main application, and finding and **isolating** the dependencies of the plugin from the main application.
This library supports .NET Core 2, but works best in .NET Core 3 and up. It allows fine-grained control over
assembly isolation and type sharing. Read [more details about type sharing below](#shared-types).

Blog post introducing this project, July 25, 2018: [.NET Core Plugins: Introducing an API for loading .dll files (and their dependencies) as 'plugins'](https://natemcmaster.com/blog/2018/07/25/netcore-plugins/). 

Since 2018, .NET Core 3
has been released which added API to improve assembly loading. If you are interested in understanding that API, see "[Create a .NET Core application with plugins][plugin-tutorial]" on docs.microsoft.com. The result of this tutorial would be simple version of DotNetCorePlugins, but missing some features like an API for unifying types across the load context boundary, hot reload, and .NET Core 2.1 support.

[plugin-tutorial]: https://docs.microsoft.com/dotnet/core/tutorials/creating-app-with-plugin-support

## Getting started

You can install the plugin loading API using the [`McMaster.NETCore.Plugins` NuGet package.][main-nuget]

```
dotnet add package McMaster.NETCore.Plugins
```

The main API to use is `PluginLoader.CreateFromAssemblyFile`.

```csharp
PluginLoader.CreateFromAssemblyFile(
    assemblyFile: "./plugins/MyPlugin/MyPlugin1.dll",
    sharedTypes: new [] { typeof(IPlugin), typeof(IServiceCollection), typeof(ILogger) },
    isUnloadable: true)
```

* assemblyFile = the file path to the main .dll of the plugin
* sharedTypes = a list of types which the loader should ensure are unified. (See [What is a shared type?](#shared-types))
* isUnloadable = (.NET Core 3+ only). Allow this plugin to be unloaded from memory at some point in the future. (Requires ensuring that you have cleaned up all usages of types from the plugin before unloading actually happens.)

See example projects in [samples/](./samples/) for more detailed, example usage.

## Usage

Using plugins requires at least two projects: (1) the 'host' app which loads plugins and (2) the plugin,
but typically also uses a third, (3) an contracts project which defines the interaction between the plugin
and the host.

For a fully functional sample of this, see [samples/hello-world/](./samples/hello-world/) .

### The plugin contract

You can define your own plugin contract. A minimal contract might look like this.

```csharp
public interface IPlugin
{
    string GetName();
}
```

There is nothing special about the name "IPlugin" or the fact that it's an interface. This is just here to illustrate a concept. Look at [samples/](./samples/) for additional examples of ways you could define the interaction between host and plugins.

### The plugins

Typically, it is best to implement plugins by targeting `netcoreapp2.0` or higher. They can target `netstandard2.0` as well, but using `netcoreapp2.0` is better because it reduces the number of redundant System.\* assemblies in the plugin output.

A minimal implementation of the plugin could be as simple as this.

```csharp
internal class MyPlugin1 : IPlugin
{
    public string GetName() => "My plugin v1";
}
```

As mentioned above, this is just an example. This library doesn't require the use of "IPlugin" or interfaces or "GetName()"
methods. This code is only here to demonstrates how you can decouple hosts and plugins, but still use interfaces for type-safe
interactions.

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

**It is important that each plugin is published into a separate directory.** This will avoid contention between plugins
and duplicate dependency issues.

You can prepare the sample plugin above by running

```
dotnet publish MyPlugin1.csproj --output plugins/MyPlugin1/
```

An implementation of a host which finds and loads this plugin might look like this. This sample uses reflection to find
all types in plugins which implement `IPlugin`, and then initializes the types' parameter-less constructors.
This is just one way to implement a host. More examples of how to use plugins can be found in [samples/](./samples/).

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

<a id="shared-types"></a>

### What is a shared type?

By default, each instance of `PluginLoader` represents a unique collection of assemblies loaded into memory.
This can make it difficult to use the plugin if you want to pass information from plugin to the host and vice versa.
Shared types allow you define the kinds of objects that will be passed between plugin and host.

For example, let's say you have a simple host app like [samples/hello-world/](./samples/hello-world/), and
two plugins which were compiled with a reference `interface IPlugin`. This interface comes from `Contracts.dll`.
When the application runs, by default, each plugin and the host will have their own version of `Contracts.dll`
which .NET Core will keep isolated.

The problem with this isolation is that an object of `IPlugin` created within the "PluginApple" or "PluginBanana" context does not appear to be an instance of `IPlugin` in any of the other plugin contexts.

![DefaultConfigDiagram](https://i.imgur.com/fHEMBO6.png)

Configuring a shared type of `IPlugin` allows the .NET to pass objects of this type across the plugin isolation
boundary. It does this by ignoring the version of `Contracts.dll` in each plugin folder, and sharing the version that comes with the Host.

![SharedTypes](https://i.imgur.com/sTGqPxa.png)

Read [even more details about shared types here](./docs/what-are-shared-types.md).

## Support for MVC and Razor

A common usage for plugins is to load class libraries that contain MVC controllers or Razor Pages. You can
set up an ASP.NET Core to load controllers and views from a plugin using the `McMaster.NETCore.Plugins.Mvc`
package.

```
dotnet add package McMaster.NETCore.Plugins.Mvc
```

The main API to use is `.AddPluginFromAssemblyFile()`, which can be chained onto the call to `.AddMvc()`
or `.AddRazorPages()` in the `Startup.ConfigureServices` method.

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var pluginFile = Path.Combine(AppContext.BaseDirectory, "plugins/MyRazorPlugin/MyRazorPlugin.dll");
        services
            .AddMvc()
            // The AddPluginFromAssemblyFile method comes from McMaster.NETCore.Plugins.Mvc
            .AddPluginFromAssemblyFile(pluginFile);
    }
}
```

See example projects in [samples/aspnetcore-mvc/](./samples/aspnetcore-mvc/) for more detailed, example usage.

## Reflection

Sometimes you may want to use a plugin along with reflection APIs such as `Type.GetType(string typeName)`
or `Assembly.Load(string assemblyString)`. Depending on where these APIs are used, they might fail to
load the assemblies in your plugin. In .NET Core 3+, there is an API which you can use to set the _ambient context_
which .NET's reflection APIs will use to load the correct assemblies from your plugin.

Example:
```c#
var loader = PluginLoader.CreateFromAssemblyFile("./plugins/MyPlugin/MyPlugin1.dll");

using (loader.EnterContextualReflection())
{
    var myPluginType = Type.GetType("MyPlugin.PluginClass");
    var myPluginAssembly = Assembly.Load("MyPlugin1");
}

```

Read [this post written by .NET Core engineers](https://github.com/dotnet/coreclr/blob/v3.0.0/Documentation/design-docs/AssemblyLoadContext.ContextualReflection.md) for even more details on contextual reflection.

## Overriding the Default Load Context

Under the hood, DotNetCorePlugins is using a .NET Core API called [ApplicationLoadContext][alc-api].
This creates a scope for resolving assemblies. By default, `PluginLoader` will create a new context
and fallback to a **default context** if it cannot find an assembly or if type sharing is enabled.
The default fallback context is inferred when `PluginLoader` is instantiated. In certain advanced scenarios,
you may need to manually change the default context, for instance, plugins which then load more plugins,
or when running .NET in a custom native host.

[alc-api]: https://docs.microsoft.com/dotnet/api/system.runtime.loader.assemblyloadcontext

To override the default assembly load context, set `PluginConfig.DefaultContext`. Example:


```csharp
AssemblyLoadContext myCustomDefaultContext = // (something).
PluginLoader.CreateFromAssemblyFile(dllPath,
     config => config.DefaultContext = myCustomDefaultContext);
```
