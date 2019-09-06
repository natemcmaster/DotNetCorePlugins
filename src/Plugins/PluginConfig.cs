// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace McMaster.NETCore.Plugins
{
    /// <summary>
    /// Represents the configuration for a .NET Core plugin.
    /// </summary>
    public class PluginConfig
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PluginConfig" />
        /// </summary>
        /// <param name="mainAssemblyPath">The full file path to the main assembly for the plugin.</param>
        public PluginConfig(string mainAssemblyPath)
        {
            if (string.IsNullOrEmpty(mainAssemblyPath))
            {
                throw new ArgumentException("Value must be null or not empty", nameof(mainAssemblyPath));
            }

            if (!Path.IsPathRooted(mainAssemblyPath))
            {
                throw new ArgumentException("Value must be an absolute file path", nameof(mainAssemblyPath));
            }

            MainAssemblyPath = mainAssemblyPath;
        }

        /// <summary>
        /// The file path to the main assembly.
        /// </summary>
        public string MainAssemblyPath { get; }

        /// <summary>
        /// A list of assemblies which should be treated as private.
        /// </summary>
        public ICollection<AssemblyName> PrivateAssemblies { get; protected set; } = new List<AssemblyName>();

        /// <summary>
        /// A list of assemblies which should be unified between the host and the plugin.
        /// </summary>
        /// <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/master/docs/what-are-shared-types.md">
        /// https://github.com/natemcmaster/DotNetCorePlugins/blob/master/docs/what-are-shared-types.md
        /// </seealso>
        public ICollection<AssemblyName> SharedAssemblies { get; protected set; } = new List<AssemblyName>();

        /// <summary>
        /// Attempt to unify all types from a plugin with the host.
        /// <para>
        /// This does not guarantee types will unify.
        /// </para>
        /// <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/master/docs/what-are-shared-types.md">
        /// https://github.com/natemcmaster/DotNetCorePlugins/blob/master/docs/what-are-shared-types.md
        /// </seealso>
        /// </summary>
        public bool PreferSharedTypes { get; set; }

#if FEATURE_UNLOAD
        private bool _isUnloadable;

        /// <summary>
        /// The plugin can be unloaded from memory.
        /// </summary>
        public bool IsUnloadable
        {
            get => _isUnloadable || EnableHotReload;
            set => _isUnloadable = value;
        }

        /// <summary>
        /// When any of the loaded files changes on disk, the plugin will be reloaded.
        /// Use the event <see cref="PluginLoader.Reloaded" /> to be notified of changes.
        /// </summary>
        public bool EnableHotReload { get; set; }
#endif
    }
}
