// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.NETCore.Plugins
{
    /// <summary>
    /// Options for how <see cref="PluginLoader"/> behaves.
    /// </summary>
    public enum PluginLoaderOptions
    {
        /// <summary>
        /// Use the default behavior.
        /// </summary>
        None,

        /// <summary>
        /// Attempt to unify all types from a plugin with the host.
        /// <para>
        /// This does not guarantee types will unify.
        /// </para>
        /// </summary>
        PreferSharedTypes,
    }
}
