// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extends
    /// </summary>
    public static class MvcPluginExtensions
    {
        /// <summary>
        /// Loads
        /// </summary>
        /// <param name="mvcBuilder">The MVC builder</param>
        /// <param name="assemblyFile">Full path the main .dll file for the plugin.</param>
        /// <returns>The builder</returns>
        public static IMvcBuilder AddPluginFromAssemblyFile(this IMvcBuilder mvcBuilder, string assemblyFile)
        {
            var plugin = PluginLoader.CreateFromAssemblyFile(
                assemblyFile, // create a plugin from for the .dll file
                config =>
                    // this ensures that the version of MVC is shared between this app and the plugin
                    config.PreferSharedTypes = true);

            var pluginAssembly = plugin.LoadDefaultAssembly();

            // This loads MVC application parts from plugin assemblies
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
            foreach (var part in partFactory.GetApplicationParts(pluginAssembly))
            {
                mvcBuilder.PartManager.ApplicationParts.Add(part);
            }

            // This piece finds and loads related parts, such as MvcAppPlugin1.Views.dll.
            var relatedAssembliesAttrs = pluginAssembly.GetCustomAttributes<RelatedAssemblyAttribute>();
            foreach (var attr in relatedAssembliesAttrs)
            {
                var assembly = plugin.LoadAssembly(attr.AssemblyFileName);
                partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                foreach (var part in partFactory.GetApplicationParts(assembly))
                {
                    mvcBuilder.PartManager.ApplicationParts.Add(part);
                }
            }

            return mvcBuilder;
        }
    }
}
