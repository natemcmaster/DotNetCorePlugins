using System;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace Microsoft.Extensions.Plugins.Tests
{
    public class PrivateDependencyTests
    {
        [Fact]
        public void EachContextHasPrivateVersions()
        {
            var json9context = PluginLoader.CreateFromConfigFile(TestResources.GetTestProjectAssembly("JsonNet9"));
            var json10context = PluginLoader.CreateFromConfigFile(TestResources.GetTestProjectAssembly("JsonNet10"));
            var json11context = PluginLoader.CreateFromConfigFile(TestResources.GetTestProjectAssembly("JsonNet11"));

            // Load newest first to prove we can load older assemblies later into the same process
            var json11 = GetJson(json11context);
            var json10 = GetJson(json10context);
            var json9 = GetJson(json9context);

            Assert.Equal(new Version("9.0.0.0"), json9.GetName().Version);
            Assert.Equal(new Version("10.0.0.0"), json10.GetName().Version);
            Assert.Equal(new Version("11.0.0.0"), json11.GetName().Version);

            // types from each context have unique identities
            Assert.NotEqual(
                json11.GetType("Newtonsoft.Json.JsonConvert", throwOnError: true),
                json10.GetType("Newtonsoft.Json.JsonConvert", throwOnError: true));
            Assert.NotEqual(
              json10.GetType("Newtonsoft.Json.JsonConvert", throwOnError: true),
              json9.GetType("Newtonsoft.Json.JsonConvert", throwOnError: true));
        }

        private Assembly GetJson(PluginLoader loader)
            => loader.LoadAssembly(new AssemblyName("Newtonsoft.Json"));
    }
}
