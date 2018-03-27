using System;
using System.Runtime.Loader;
using Xunit;
using Xunit.Sdk;

namespace Loader.Tests
{
    public class UnifiedTypeTests
    {
        [Fact]
        public void ItUnifiesOnTypes()
        {
            var samplePath = TestResources.GetTestProjectAssembly("XunitSample");
            var unifiedLoader = new AssemblyLoaderBuilder()
                .AddSearchPath(samplePath)
                .AddManagedLibrary("XunitSample", samplePath)
                .UnifyWith(typeof(XunitTestCase).Assembly)
                .Build();

            var unifiedVersion = GetXunitVersion(unifiedLoader);
            Assert.NotEqual(new Version("2.2.0.3545"), unifiedVersion);
            Assert.Equal(typeof(XunitTestCase).Assembly.GetName().Version, unifiedVersion);
        }

        [Fact]
        public void DoesNotUnifyByDefault()
        {
            var samplePath = TestResources.GetTestProjectAssembly("XunitSample");
            var loader = AssemblyLoader.CreateFromFile(samplePath);
            Assert.Equal(new Version("2.2.0.3545"), GetXunitVersion(loader));
        }

        [Fact]
        public void ItCanDowngradeUnifiedTypes()
        {
            var samplePath = TestResources.GetTestProjectAssembly("NetCoreApp20App");
            var defaultLoader = new AssemblyLoaderBuilder()
               .AddSearchPath(samplePath)
               .AddManagedLibrary("NetCoreApp20App", samplePath)
               .AddManagedLibrary("Test.Referenced.Library", TestResources.GetTestProjectAssembly("Test.Referenced.Library"))
               .Build();
            var unifedLoader = new AssemblyLoaderBuilder()
              .AddSearchPath(samplePath)
              .AddManagedLibrary("NetCoreApp20App", samplePath)
              .AddManagedLibrary("Test.Referenced.Library", TestResources.GetTestProjectAssembly("Test.Referenced.Library"))
              .UnifyWith(typeof(Test.Referenced.Library.Class1).Assembly)
              .Build();

            Assert.Equal(new Version("2.0.0.0"), defaultLoader.LoadAssembly("Test.Referenced.Library").GetName().Version);
            Assert.Equal(new Version("1.0.0.0"), unifedLoader.LoadAssembly("Test.Referenced.Library").GetName().Version);
        }

        private Version GetXunitVersion(AssemblyLoader loader)
            => loader.LoadAssembly("xunit.execution.dotnet").GetName().Version;
    }
}
