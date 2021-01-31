// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HelloWorld;
using McMaster.NETCore.Plugins;

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
            sharedTypes: new[] { typeof(IPlugin) });
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
        var plugin = Activator.CreateInstance(pluginType) as IPlugin;

        Console.WriteLine($"Created plugin instance '{plugin?.GetName()}'.");
    }
}
