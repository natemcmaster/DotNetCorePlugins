.NET Core Plugins - design doc
==============================

This repository contains a library code to support a .NET Core plugin model.

## Synopsis

This library is designed to provide support to .NET Core applications that wish to load assemblies on the fly and execute them as extensions to the main application.
It defines a plugin model that allows these applications to control, to some level, isolation between the extension (plugin) loaded and the
host (main application).

## Background

Today, implementing a plugin model requires a high level of knowledge about how assemblies are resolved and loaded. Various features in .NET Core exist to assist in this, but none of them provide an API and experience consistent with how corehost handles and loads assemblies.

Existing features:
  - System.Runtime.Loader.AssemblyLoadContext - users can implement custom assembly loading behaviors
  - additionalDeps - users can craft a deps.json file which the host will load into all processes as if the dependencies were in the application's .deps.json file

Problems with existing features:
  - AssemblyLoadContext - users must implement all load behavior themselves. If users want support for deps.json files, additional probing paths, RID graphs, and more, they must implement it themselves. As corehost continually changes, these behaviors must be updated.
  - additionalDeps - this feature is currently too inflexible. Applciations can control when or how additional dependencies are loaded, and also, it forces unification to a single version of a dependency.

## What this library does

This library provides API for .NET Core app developers to load assemblies as plugins. This API should:

 - reduce the complexity of loading assemblies
 - allow plugins to define additional dependencies
 - allow controlling the behavior of how types are unified between the host and plugin
 - allow the host to customize how and when to load applications
 - (stretch goal) allow unloading plugins (depends on unloadable AssemblyLoadContext's)

## Implementation

This API implements these features using:
 - AssemblyLoadContext to manage assembly loading and assembly version isolation
 - Microsoft.Extensions.DependencyModel to use the .deps.json and .runtimeconfig.json files to express additional dependencies
and search paths for dependencies

## Sample usage

A host and plugin have a shared abstraction
```c#
public interface IFruit
{
    string GetColor()
}
```

A host application could load plugins like this:

```c#
// (pseudocode)
public class Program
{
    public static void Main(string[] args)
    {
        foreach (var pluginDirectory in Directory.GetDirectories("plugins/"))
        {
            var pluginDirName = Path.GetFileName(pluginDir);
            var assemblyFile = Path.Combine(pluginDir, pluginDirName + ".dll");
            var loader = PluginLoader.CreateFromAssemblyFile(
                assemblyFile: pluginFile,
                sharedTypes: new [] { typeof(IFruit) });

            var plugin = loader.LoadDefaultAssembly();
            foreach (var fruitType in plugin.GetTypes().Where(t => typeof(IFruit).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var fruit = (IFruit)Activator.CreateInstance(fruitType, new object[0]);
                Console.WriteLine(fruit.GetColor());
            }

            loader.Dispose();
        }
    }
}
```

A plugin author could implement the shared abstraction and distribute a plugin without knowing how the host will behave:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>
```
```c#
internal class MyApple : IFruit
{
    public string GetColor() => "Red";
}
```
