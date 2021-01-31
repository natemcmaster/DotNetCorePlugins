// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Contracts;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Host
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            var loaders = GetPluginLoaders();

            ConfigureServices(services, loaders);

            var serviceProvider = services.BuildServiceProvider();

            var mixer = serviceProvider.GetRequiredService<IMixerService>();
            mixer.MixIt();
        }

        private static List<PluginLoader> GetPluginLoaders()
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
                        sharedTypes: new[] { typeof(IPluginFactory), typeof(IServiceCollection) });
                    loaders.Add(loader);
                }
            }

            return loaders;
        }

        private static void ConfigureServices(ServiceCollection services, List<PluginLoader> loaders)
        {
            // Create an instance of plugin types
            foreach (var loader in loaders)
            {
                foreach (var pluginType in loader
                    .LoadDefaultAssembly()
                    .GetTypes()
                    .Where(t => typeof(IPluginFactory).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    // This assumes the implementation of IPluginFactory has a parameterless constructor
                    var plugin = Activator.CreateInstance(pluginType) as IPluginFactory;

                    plugin?.Configure(services);
                }
            }
        }
    }
}
