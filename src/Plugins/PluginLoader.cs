// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using McMaster.NETCore.Plugins.Loader;

namespace McMaster.NETCore.Plugins
{
    /// <summary>
    /// This loader attempts to load binaries for execution (both managed assemblies and native libraries)
    /// in the same way that .NET Core would if they were originally part of the .NET Core application.
    /// <para>
    /// This loader reads configuration files produced by .NET Core (.deps.json and runtimeconfig.json)
    /// as well as a custom file (*.config files). These files describe a list of .dlls and a set of dependencies.
    /// The loader searches the plugin path, as well as any additionally specified paths, for binaries
    /// which satisfy the plugin's requirements.
    /// </para>
    /// </summary>
    public class PluginLoader
    {
        /// <summary>
        /// Create a plugin loader using the settings from a plugin config file.
        /// <seealso cref="PluginConfig" /> for defaults on the plugin configuration.
        /// </summary>
        /// <param name="filePath">The file path to the plugin config.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromConfigFile(string filePath, Type[] sharedTypes = null)
        {
            var config = PluginConfig.CreateFromFile(filePath);
            var baseDir = Path.GetDirectoryName(filePath);
            return new PluginLoader(config, baseDir, sharedTypes);
        }

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the plugin config.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes = null)
        {
            var config = new FileOnlyPluginConfig(assemblyFile);
            var baseDir = Path.GetDirectoryName(assemblyFile);
            return new PluginLoader(config, baseDir, sharedTypes);
        }

        private class FileOnlyPluginConfig : PluginConfig
        {
            public FileOnlyPluginConfig(string filePath)
                : base(new AssemblyName(Path.GetFileNameWithoutExtension(filePath)), Array.Empty<AssemblyName>())
            { }
        }

        private readonly string _mainAssembly;
        private AssemblyLoadContext _context;

        /// <summary>
        /// Load the main assembly for the plugin.
        /// </summary>
        public Assembly LoadDefaultAssembly()
        => _context.LoadFromAssemblyPath(_mainAssembly);

        /// <summary>
        /// Load an assembly by name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssembly(AssemblyName assemblyName)
            => _context.LoadFromAssemblyName(assemblyName);

        /// <summary>
        /// Load an assembly by name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssembly(string assemblyName)
            => LoadAssembly(new AssemblyName(assemblyName));

        internal PluginLoader(PluginConfig config, string baseDir, Type[] sharedTypes)
        {
            _mainAssembly = Path.Combine(baseDir, config.MainAssembly.Name + ".dll");
            _context = CreateLoadContext(baseDir, config, sharedTypes);
        }

        private static AssemblyLoadContext CreateLoadContext(
            string baseDir,
            PluginConfig config,
            Type[] sharedTypes)
        {
            var depsJsonFile = Path.Combine(baseDir, config.MainAssembly.Name + ".deps.json");

            var builder = new AssemblyLoadContextBuilder();

            builder.TryAddDependencyContext(depsJsonFile, out _);
            builder.SetBaseDirectory(baseDir);

            foreach (var ext in config.PrivateAssemblies)
            {
                builder.PreferLoadContextAssembly(ext);
            }

            if (sharedTypes != null)
            {
                foreach (var type in sharedTypes)
                {
                    builder.PreferDefaultLoadContextAssembly(type.Assembly.GetName());
                }
            }

            var runtimeConfigFile = Path.Combine(baseDir, config.MainAssembly.Name + ".runtimeconfig.json");

            builder.TryAddAdditionalProbingPathFromRuntimeConfig(runtimeConfigFile, includeDevConfig: true, out _);

            return builder.Build();
        }
    }
}
