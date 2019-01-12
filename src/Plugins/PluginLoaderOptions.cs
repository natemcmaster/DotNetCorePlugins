// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.NETCore.Plugins
{
    /// <summary>
    /// Options for how <see cref="PluginLoader"/> behaves.
    /// </summary>
    [Flags]
    public enum PluginLoaderOptions
    {
        /// <summary>
        /// Use the default behavior.
        /// </summary>
        None = 0,

        /// <summary>
        /// Attempt to unify all types from a plugin with the host.
        /// <para>
        /// This does not guarantee types will unify.
        /// </para>
        /// </summary>
        PreferSharedTypes = 1 << 0,

#if FEATURE_UNLOAD
        /// <summary>
        /// If the platform supports it, allow unloading the plugin. This requires .NET Core 3.0 or higher.
        /// <para>
        /// Setting this option does not guarantee that the plugin can be unloaded.
        /// </para>
        /// </summary>
        IsUnloadable = 1 << 1,
#endif
    }
}
