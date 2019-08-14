ASP.NET Core Sample
===================

This sample contains 4 projects which demonstrate a simple plugin scenario.

1. 'Abstractions' defines common interfaces shared by the web application (host) and plugins
2. 'MainWebApp' is an ASP.NET Core application which scans for a 'plugins' folder in its base directory and attempts to load any plugins it finds
3. 'WebAppPlugin1' references 'Abstractions' and implements `IWebPlugin`. This plugin has a dependency on [AutoMapper](https://www.nuget.org/packages/AutoMapper/) version 6.
4. 'WebAppPlugin2' is the same as plugin1, but it uses AutoMapper version 7.

Normally, in .NET Core applications you cannot reference two different versions of the same assembly.
However, as this sample demonstrates, using .NET Core plugins you can load and use two different versions.

* http://localhost:5000/plugin/v1 responds with
```
This plugin uses AutoMapper, Version=6.2.2.0, Culture=neutral, PublicKeyToken=be96cd2c38ef1005
```

* http://localhost:5000/plugin/v2 responds with
```
This plugin uses AutoMapper, Version=7.0.1.0, Culture=neutral, PublicKeyToken=be96cd2c38ef1005
```

There are some important types, however, which must share the same identity between the plugins and the host.
To ensure type exchange works between the host and the plugins, the MainWebApp project uses the `sharedTypes`
parameter on `PluginLoader.CreateFromAssemblyFile`.

```csharp
    var loader = PluginLoader.CreateFromAssemblyFile(
        pluginAssembly,
        sharedTypes: new[]
        {
            typeof(IApplicationBuilder),
            typeof(IWebPlugin),
            typeof(IServiceCollection),
        });
```

This is important because the plugins in this sample are compiled for ASP.NET Core 2.0 interfaces,
but the MainWebApp uses ASP.NET Core 2.1. If not for this parameter, the plugins would also attempt to use
a private copy of the ASP.NET Core implementations and type exchange between the plugin and the web app
would fail to resolve `IApplicationBuilder` and `IServiceCollection` as the same type.
