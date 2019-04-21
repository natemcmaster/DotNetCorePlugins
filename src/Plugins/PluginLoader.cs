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
    public class PluginLoader : IDisposable
    {
        // we have to duplicate a large block of xml code because C# doesn't allow conditional XML elements
#if FEATURE_UNLOAD
        /// <summary>
        /// Create a plugin loader using the settings from a plugin config file.
        /// <seealso cref="PluginConfig" /> for defaults on the plugin configuration.
        /// </summary>
        /// <param name="filePath">The file path to the plugin config.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <param name="isUnloadable">Enable unloading the plugin from memory.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromConfigFile(string filePath, Type[] sharedTypes = null, bool isUnloadable = false)
        {
            var loaderOptions = isUnloadable
                        ? PluginLoaderOptions.IsUnloadable
                        : PluginLoaderOptions.None;
#else
        /// <summary>
        /// Create a plugin loader using the settings from a plugin config file.
        /// <seealso cref="PluginConfig" /> for defaults on the plugin configuration.
        /// </summary>
        /// <param name="filePath">The file path to the plugin config.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromConfigFile(string filePath, Type[] sharedTypes = null)
        {
            var loaderOptions = PluginLoaderOptions.None;
#endif
            var config = PluginConfig.CreateFromFile(filePath);
            var baseDir = Path.GetDirectoryName(filePath);
            return new PluginLoader(config,
                baseDir,
                sharedTypes,
                loaderOptions);
        }

#if FEATURE_UNLOAD
        /// <summary>
        /// Create a plugin loader using an existing <see cref="PluginConfig"/> instance.
        /// </summary>
        /// <param name="config">The <see cref="PluginConfig"/> instance.</param>
        /// <param name="baseDir">The base directory from which to load / search for dependencies on disk.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <param name="isUnloadable">Enable unloading the plugin from memory.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromConfigFile(PluginConfig config, string baseDir, Type[] sharedTypes = null, bool isUnloadable = false)
        {
            var loaderOptions = isUnloadable
                        ? PluginLoaderOptions.IsUnloadable
                        : PluginLoaderOptions.None;
#else
        /// <summary>
        /// Create a plugin loader using an existing <see cref="PluginConfig"/> instance.
        /// </summary>
        /// <param name="config">The <see cref="PluginConfig"/> instance.</param>
        /// <param name="baseDir">The base directory from which to load / search for dependencies on disk.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromConfigFile(PluginConfig config, string baseDir, Type[] sharedTypes = null)
        {
            var loaderOptions = PluginLoaderOptions.None;
#endif
            return new PluginLoader(config,
                baseDir,
                sharedTypes,
                loaderOptions);
        }

#if FEATURE_UNLOAD
        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the plugin config.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <param name="isUnloadable">Enable unloading the plugin from memory.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes = null, bool isUnloadable = false)
        {
            var loaderOptions = isUnloadable
                        ? PluginLoaderOptions.IsUnloadable
                        : PluginLoaderOptions.None;
#else
        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the plugin config.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes = null)
        {
            var loaderOptions = PluginLoaderOptions.None;
#endif
            return CreateFromAssemblyFile(assemblyFile,
                    sharedTypes,
                    loaderOptions);
        }

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the plugin config.</param>
        /// <param name="sharedTypes">A list of types which should be shared between the host and the plugin.</param>
        /// <param name="loaderOptions">Options for the loader</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes, PluginLoaderOptions loaderOptions)
        {
            var config = new FileOnlyPluginConfig(assemblyFile);
            var baseDir = Path.GetDirectoryName(assemblyFile);
            return new PluginLoader(config, baseDir, sharedTypes, loaderOptions);
        }

        private class FileOnlyPluginConfig : PluginConfig
        {
            public FileOnlyPluginConfig(string filePath)
                : base(new AssemblyName(Path.GetFileNameWithoutExtension(filePath)), Array.Empty<AssemblyName>())
            { }
        }

        private readonly string _mainAssembly;
        private readonly AssemblyLoadContext _context;
        private volatile bool _disposed;

        internal PluginLoader(PluginConfig config,
            string baseDir,
            Type[] sharedTypes,
            PluginLoaderOptions loaderOptions)
        {
            _mainAssembly = Path.Combine(baseDir, config.MainAssembly.Name + ".dll");
            _context = CreateLoadContext(baseDir, config, sharedTypes, loaderOptions);
        }

        /// <summary>
        /// True when this plugin is capable of being unloaded.
        /// </summary>
        public bool IsUnloadable
        {
            get
            {
#if FEATURE_UNLOAD
                return _context.IsCollectible;
#else
                return false;
#endif
            }
        }

        internal AssemblyLoadContext LoadContext => _context;

        /// <summary>
        /// Load the main assembly for the plugin.
        /// </summary>
        public Assembly LoadDefaultAssembly()
        {
            EnsureNotDisposed();
            return _context.LoadFromAssemblyPath(_mainAssembly);
        }

        /// <summary>
        /// Load an assembly by name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssembly(AssemblyName assemblyName)
        {
            EnsureNotDisposed();
            return _context.LoadFromAssemblyName(assemblyName);
        }

        /// <summary>
        /// Load an assembly by name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssembly(string assemblyName)
        {
            EnsureNotDisposed();
            return LoadAssembly(new AssemblyName(assemblyName));
        }

        /// <summary>
        /// Disposes the plugin loader. This only does something if <see cref="IsUnloadable" /> is true.
        /// When true, this will unload assemblies which which were loaded during the lifetime
        /// of the plugin.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

#if FEATURE_UNLOAD
            if (_context.IsCollectible)
            {
                _context.Unload();
            }
#endif
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PluginLoader));
            }
        }

        private static AssemblyLoadContext CreateLoadContext(
            string baseDir,
            PluginConfig config,
            Type[] sharedTypes,
            PluginLoaderOptions loaderOptions)
        {
            var depsJsonFile = Path.Combine(baseDir, config.MainAssembly.Name + ".deps.json");

            var builder = new AssemblyLoadContextBuilder();

            if (File.Exists(depsJsonFile))
            {
                builder.AddDependencyContext(depsJsonFile);
            }

            builder.SetBaseDirectory(baseDir);

            foreach (var ext in config.PrivateAssemblies)
            {
                builder.PreferLoadContextAssembly(ext);
            }

            if (loaderOptions.HasFlag(PluginLoaderOptions.PreferSharedTypes))
            {
                builder.PreferDefaultLoadContext(true);
            }

#if FEATURE_UNLOAD
            if (loaderOptions.HasFlag(PluginLoaderOptions.IsUnloadable))
            {
                builder.EnableUnloading();
            }
#endif

            if (sharedTypes != null)
            {
                foreach (var type in sharedTypes)
                {
                    builder.PreferDefaultLoadContextAssembly(type.Assembly.GetName());
                }
            }

            var pluginRuntimeConfigFile = Path.Combine(baseDir, config.MainAssembly.Name + ".runtimeconfig.json");

            builder.TryAddAdditionalProbingPathFromRuntimeConfig(pluginRuntimeConfigFile, includeDevConfig: true, out _);

            // Always include runtimeconfig.json from the host app.
            // in some cases, like `dotnet test`, the entry assembly does not actually match with the
            // runtime config file which is why we search for all files matching this extensions.
            foreach (var runtimeconfig in Directory.GetFiles(AppContext.BaseDirectory, "*.runtimeconfig.json"))
            {
                builder.TryAddAdditionalProbingPathFromRuntimeConfig(runtimeconfig, includeDevConfig: true, out _);
            }

            return builder.Build();
        }
    }
}
