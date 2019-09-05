using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Test.Referenced.Library;
using Test.Shared.Abstraction;
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
        [Fact]
        public void TransitiveAssembliesOfSharedTypesAreResolved()
        {
            using var loader = PluginLoader.CreateFromAssemblyFile(
                    TestResources.GetTestProjectAssembly("TransitivePlugin"),
                    sharedTypes: new[] { typeof(SharedType) });

            var assembly = loader.LoadDefaultAssembly();
            var configType = assembly.GetType("TransitivePlugin.PluginConfig", throwOnError: true)!;
            var config = Activator.CreateInstance(configType);
            var transitiveInstance = configType.GetMethod("GetTransitiveType")?.Invoke(config, null);
            Assert.IsType<Test.Transitive.TransitiveSharedType>(transitiveInstance);
        }
    }
}
