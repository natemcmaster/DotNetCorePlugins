using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using McMaster.NETCore.Plugins.Loader;
using Xunit;

namespace McMaster.NETCore.Plugins.Tests
{
    public class ManagedLoadContextTests
    {
        [Fact]
        public void ItUpgradesTypesInContext()
        {
            var samplePath = TestResources.GetTestProjectAssembly("XunitSample");
            var context = new AssemblyLoadContextBuilder()
                .AddProbingPath(samplePath)
                .AddDependencyContext(Path.Combine(Path.GetDirectoryName(samplePath), "XunitSample.deps.json"))
                .PreferDefaultLoadContext(true)
                .Build();

            Assert.Same(typeof(TheoryData).Assembly, LoadAssembly(context, "xunit.core"));
        }

        [Fact]
        public void ContextsHavePrivateVersionsByDefault()
        {
            var samplePath = TestResources.GetTestProjectAssembly("XunitSample");

            var context = new AssemblyLoadContextBuilder()
               .AddProbingPath(samplePath)
               .AddDependencyContext(Path.Combine(Path.GetDirectoryName(samplePath), "XunitSample.deps.json"))
               .Build();

            Assert.NotSame(typeof(TheoryData).Assembly, LoadAssembly(context, "xunit.core"));
        }

        [Fact]
        public void ItCanDowngradeUnifiedTypes()
        {
            var samplePath = TestResources.GetTestProjectAssembly("NetCoreApp20App");

            var defaultLoader = new AssemblyLoadContextBuilder()
               .AddProbingPath(samplePath)
               .PreferDefaultLoadContext(false)
               .AddDependencyContext(Path.Combine(Path.GetDirectoryName(samplePath), "NetCoreApp20App.deps.json"))
               .Build();

            var unifedLoader = new AssemblyLoadContextBuilder()
              .AddProbingPath(samplePath)
              .PreferDefaultLoadContext(true)
              .AddDependencyContext(Path.Combine(Path.GetDirectoryName(samplePath), "NetCoreApp20App.deps.json"))
              .Build();

            Assert.Equal(new Version("2.0.0.0"), LoadAssembly(defaultLoader, "Test.Referenced.Library").GetName().Version);
            Assert.Equal(new Version("1.0.0.0"), LoadAssembly(unifedLoader, "Test.Referenced.Library").GetName().Version);
        }

        private Assembly LoadAssembly(AssemblyLoadContext context, string name)
            => context.LoadFromAssemblyName(new AssemblyName(name));
    }
}
