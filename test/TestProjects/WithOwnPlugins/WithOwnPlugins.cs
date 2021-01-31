// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using McMaster.NETCore.Plugins;
using WithOurPluginsPluginContract;
using WithOwnPluginsContract;

namespace WithOwnPlugins
{
    public class WithOwnPlugins : IWithOwnPlugins
    {
        public bool TryLoadPluginsInCustomContext(AssemblyLoadContext? callingContext)
        {
            var currentContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
            if (currentContext == callingContext)
            {
                throw new ArgumentException("The context of the caller is the context of this assembly. This invalidates the test.");
            }

#if !NETCOREAPP2_1
            /*
                Ensure the source calling context does not have our plugin's interfaces loaded.
                This guarantees that the Assembly cannot possibly unify with the default load context.

                Note:
                The code below this check would fail anyway if the assembly would unify with the default context.
                This is more of a safety check to ensure "correctness" as opposed to anything else.
             */
            var sayHelloAssembly = typeof(ISayHello).Assembly;
            if (callingContext?.Assemblies.Contains(sayHelloAssembly) == true) // .Assemblies API not available in Core 2.X
            {
                throw new ArgumentException("The context of the caller has this plugin's interface to interact with its own plugins loaded. Test is void.");
            }
#endif

            // Load our own plugins: Remember, we are in an isolated, non-default ALC.
            var plugins = new List<ISayHello?>();
            string[] assemblyNames = { "Plugins/WithOurPluginsPluginA.dll", "Plugins/WithOurPluginsPluginB.dll" };

            foreach (var assemblyName in assemblyNames)
            {
                var currentAssemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("Unable to get folder path for currently executing assembly.");
                var pluginPath = Path.Combine(currentAssemblyFolderPath, assemblyName);

                using var loader = PluginLoader.CreateFromAssemblyFile(pluginPath, new[] { typeof(ISayHello) });
                var assembly = loader.LoadDefaultAssembly();
                var configType = assembly.GetTypes().First(x => typeof(ISayHello).IsAssignableFrom(x) && !x.IsAbstract);
                var plugin = (ISayHello?)Activator.CreateInstance(configType);
                if (plugin == null)
                {
                    throw new Exception($"Failed to load instance of {nameof(ISayHello)} from plugin.");
                }

                plugins.Add(plugin);
            }

            // Shouldn't need to check for this but just in case to absolutely make sure.
            if (plugins.Any(plugin => String.IsNullOrEmpty(plugin?.SayHello())))
            {
                throw new Exception("No value returned from plugin.");
            }

            return true;
        }
    }
}
