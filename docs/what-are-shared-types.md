# An Explanation of Shared Types

The PluginLoader API uses the term "shared types". This document explains what it means.

```csharp
PluginLoader.CreateFromAssemblyFile("./plugins/MyPlugin/MyPlugin1.dll",
    sharedTypes: new [] { typeof(ILogger) });
    
// versus

PluginLoader.CreateFromAssemblyFile("./plugins/MyPlugin/MyPlugin1.dll",
    config => config.PreferSharedTypes = true);
```

## Concepts

First, a quick overview of essential concepts.

#### Type identity

Type identity what makes a class/struct/enum unique. It is defined by the combination of type name (which includes its namespace), assembly name, 
assembly public key token, and assembly version. You can inspect a type's identity in .NET by looking at System.Type.AssemblyQualifiedName. 

For example,

```csharp
typeof(ILogger).AssemblyQualifiedName 
   => "Microsoft.Extensions.Logging.ILogger, Microsoft.Extensions.Logging.Abstractions, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60"
```

Element                    | Value
---------------------------|------------------
Type name                  | Microsoft.Extensions.Logging.ILogger
Assembly name              | Microsoft.Extensions.Logging.Abstractions
Assembly version           | 2.2.0.0
Assembly public key token  | adb9793829ddae60

> Simplification: we're going to ignore the "Culture" part of the fully qualified name for now.

#### Diamond dependencies

The [diamond dependency problem][dep-hell] is when a library A depends on libraries B and C, both B and C depend on library D, 
but B requires version D.1 and C requires version D.2.

[dep-hell]: https://en.wikipedia.org/wiki/Dependency_hell

<img width="250" title="Diamond dependency" src="https://imgur.com/WEA8X1U.png" />

You could solve this problem by

1. Choosing D.1
1. Choosing D.2
1. Choosing both

#### Type unification (option 2)

Type unification is .NET's solution for the diamond dependency problem. In the simple example above,
.NET's build system picks the higher version (D.2) and writes this into the application manifest 
(the .deps.json or .config file in build output.) Then, when the application is running and encounters usages of D.1, .NET binds
the usage to D.2 instead.

In other words, .NET will ignore assembly version when evaluating type identity.

* Type name
* Assembly name
* ~~Assembly version~~ _this part gets ignored_
* Assembly public key token

Why is this done? It allows **type exchange** so code can share instances of types even if the code was originally compiled 
with different dependency versions.

```csharp
var instanceOfD = new D(); // D.2
new B().DoSomethingWith(instanceOfD); // Library B was compiled to expect D.1, but type unification makes it work with D.2
new C().DoSomethingWith(instanceOfD); 
```

## But what if...

There are two common problems with type unification.

1. Breaking changes: what if library B depends on a behavior of D.1 that changed in D.2 and breaks B? Conversely, what library C uses a new API added in D.2, 
   but we force the app to use D.1 instead?
2. Static vs dynamic: what if my app has a plugin system with dynamic dependencies? 

## This library's answer...

By default, this `PluginLoader` does not unify any types. This means you can have **multiple versions of the same assembly** 
loaded in separate plugins.

```csharp
PluginLoader.CreateFromAssemblyFile("./plugins/MyPlugin/MyPlugin1.dll")
```

This can make working with a plugin difficult because it breaks **type exchange**, so the `sharedTypes` list API
is provided to allow you to select which types you want to make sure are unified between the plugin and the 
application loading the plugin (aka the host).
```csharp
PluginLoader.CreateFromAssemblyFile("./plugins/MyPlugin/MyPlugin1.dll",
    sharedTypes: new [] { typeof(ILogger) });
```

Finally, you can invert the default completely to **always attempt to unify** by setting `PreferSharedTypes`. In this mode,
the assembly version provided by the host uses is always used.
```csharp
PluginLoader.CreateFromAssemblyFile("./plugins/MyPlugin/MyPlugin1.dll",
    config => config.PreferSharedTypes = true);

// In older versions of the library, this API was found on PluginLoaderOptions
PluginLoader.CreateFromAssemblyFile("./plugins/MyPlugin/MyPlugin1.dll",
    PluginLoaderOptions.PreferSharedTypes);
```
