#if NETCOREAPP3_1

using Xunit;

namespace McMaster.NETCore.Plugins.Tests
{
    public class ShadowCopyTests
    {
        [Fact]
        public void DoesNotThrowWhenLoadingSameNativeDependecyMoreThanOnce()
        {
            var samplePath = TestResources.GetTestProjectAssembly("NativeDependency");

            using var loader = PluginLoader
                .CreateFromAssemblyFile(samplePath, config => config.EnableHotReload = true);

            var nativeDependecyLoadMethod = loader.LoadDefaultAssembly()
                .GetType("NativeDependency.NativeDependencyLoader")
                .GetMethod("Load");

            var exception = Record.Exception(() => nativeDependecyLoadMethod.Invoke(null, null));

            Assert.Null(exception);
        }
    }
}

#endif
