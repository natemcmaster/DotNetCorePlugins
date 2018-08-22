using System;
using System.Reflection;
using Xunit;

namespace McMaster.NETCore.Plugins.Tests
{
    public class BasicAssemblyLoaderTests
    {
        [Fact]
        public void LoadsNetCoreProjectWithNativeDeps()
        {
            var path = TestResources.GetTestProjectAssembly("PowerShellPlugin");
            var loader = PluginLoader.CreateFromConfigFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var method = assembly
                .GetType("PowerShellPlugin.Program", throwOnError: true)
                .GetMethod("GetGreeting", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal("hello", method.Invoke(null, Array.Empty<object>()));
        }

        [Fact]
        public void LoadsNetCoreApp20Project()
        {
            var path = TestResources.GetTestProjectAssembly("NetCoreApp20App");
            var loader = PluginLoader.CreateFromConfigFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var method = assembly
                .GetType("NetCoreApp20App.Program", throwOnError: true)
                .GetMethod("GetGreeting", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal("Hello world!", method.Invoke(null, Array.Empty<object>()));
        }

        [Fact]
        public void LoadsNetStandard20Project()
        {
            var path = TestResources.GetTestProjectAssembly("NetStandardClassLib");
            var loader = PluginLoader.CreateFromConfigFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var type = assembly.GetType("NetStandardClassLib.Class1", throwOnError: true);
            var method = type.GetMethod("GetColor", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal("Red", method.Invoke(Activator.CreateInstance(type), Array.Empty<object>()));
        }
    }
}
