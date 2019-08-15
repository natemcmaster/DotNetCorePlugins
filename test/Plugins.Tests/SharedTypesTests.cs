using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Test.Referenced.Library;
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
    }
}
