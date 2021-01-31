// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Test.Referenced.Library;
using Test.Shared.Abstraction;
using WithOwnPluginsContract;
using Xunit;

namespace McMaster.NETCore.Plugins.Tests
{
    public class SharedTypesTests
    {
        [Fact]
        public void PluginsCanForceSharedTypes()
        {
            var pluginsNames = new[] { "Banana", "Strawberry" };
            var loaders = new List<PluginLoader>();
            foreach (var name in pluginsNames)
            {
                var loader = PluginLoader.CreateFromAssemblyFile(
                    TestResources.GetTestProjectAssembly(name),
                    sharedTypes: new[] { typeof(IFruit) });
                loaders.Add(loader);
            }

            foreach (var plugin in loaders.Select(l => l.LoadDefaultAssembly()))
            {
                var fruitType = Assert.Single(plugin.GetTypes(), t => typeof(IFruit).IsAssignableFrom(t));
                var fruit = (IFruit)Activator.CreateInstance(fruitType)!;
                Assert.NotNull(fruit.GetFlavor());
            }
        }

        /// <summary>
        /// This is a carefully crafted example which tests
        /// that the assembly dependencies of shared types are
        /// accounted for. Without this, the order in which code loads
        /// could cause different assembly versions to be loaded.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TransitiveAssembliesOfSharedTypesAreResolved(bool isLazyLoaded)
        {
            using var loader = PluginLoader.CreateFromAssemblyFile(TestResources.GetTestProjectAssembly("TransitivePlugin"), sharedTypes: new[] { typeof(SharedType) }, config => config.IsLazyLoaded = isLazyLoaded);
            var assembly = loader.LoadDefaultAssembly();
            var configType = assembly.GetType("TransitivePlugin.PluginConfig", throwOnError: true)!;
            var config = Activator.CreateInstance(configType);
            var transitiveInstance = configType.GetMethod("GetTransitiveType")?.Invoke(config, null);
            Assert.IsType<Test.Transitive.TransitiveSharedType>(transitiveInstance);
        }

        /// <summary>
        /// This is a carefully crafted example which tests
        /// whether the library can be used outside of the default load context
        /// (<see cref="AssemblyLoadContext.Default"/>).
        ///
        /// It works by loading a plugin (that gets loaded into another ALC)
        /// which in turn loads its own plugins using the library. If said plugin
        /// can successfully share its own types, the test should work.
        /// </summary>
        [Fact]
        public void NonDefaultLoadContextsAreSupported()
        {
            /* The loaded plugin here will be in its own ALC.
             * It will load its own plugins, which are not known to this ALC.
             * Then this ALC will ask that ALC if it managed to successfully its own plugins.
             */

            using var loader = PluginLoader.CreateFromAssemblyFile(TestResources.GetTestProjectAssembly("WithOwnPlugins"), new[] { typeof(IWithOwnPlugins) });
            var assembly = loader.LoadDefaultAssembly();
            var configType = assembly.GetType("WithOwnPlugins.WithOwnPlugins", throwOnError: true)!;
            var config = (IWithOwnPlugins?)Activator.CreateInstance(configType);

            /*
             * Here, we have made sure that neither WithOwnPlugins or its own plugins have any way to be
             * accidentally unified with the default (current for our tests) ALC. We did this by ensuring they are
             * not loaded in the default ALC in the first place, hence the use of the `IWithOwnPlugins` interface.
             *
             * We are simulating a real use case scenario where the plugin host is 100% unaware of the
             * plugin's own plugins.
             *
             * An important additional note:
             * - Although the assembly of WithOurPlugins is not directly referenced thanks to the
             *   ReferenceOutputAssembly = false property, its contents will still be copied to the output.
             * - This is problematic because the test runner seems to load all of the Assemblies present in the same
             *   directory as the test assembly, regardless of whether referenced or not.
             * - Therefore we store the plugins of `WithOwnPlugins` are output in a `Plugins` directory.
             *   (see csproj of WithOwnPlugins, Link property)
             *
             * You can ensure that WithOwnPlugins or its plugins are not loaded by inspecting the following:
             * AssemblyLoadContext.Default.Assemblies
             *
             * Even if it was loaded, there's an extra check on the other side to ensure no unification could happen.
             * Nothing wrong with being extra careful ;).
             */

            var callingContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
            Assert.True(config?.TryLoadPluginsInCustomContext(callingContext));
        }
    }
}
