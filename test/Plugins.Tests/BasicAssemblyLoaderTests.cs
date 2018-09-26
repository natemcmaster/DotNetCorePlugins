using System;
using System.Reflection;
using McMaster.Extensions.Xunit;
using Test.Referenced.Library;
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

        [Fact]
        [UseCulture("es")]
        public void ItLoadsSatelliteAssemblies()
        {
            var fruit = GetPlátano();
            Assert.Equal("Plátano", fruit.GetFlavor());
        }

        [Fact]
        [UseCulture("en")]
        public void ItLoadsDefaultCultureAssemblies()
        {
            var fruit = GetPlátano();
            Assert.Equal("Banana", fruit.GetFlavor());
        }

        private IFruit GetPlátano()
        {
            var path = TestResources.GetTestProjectAssembly("Plátano");
            var loader = PluginLoader.CreateFromConfigFile(path, sharedTypes: new[] { typeof(IFruit) });
            var assembly = loader.LoadDefaultAssembly();
            var type = Assert.Single(assembly.GetTypes(), t => typeof(IFruit).IsAssignableFrom(t));
            return (IFruit) Activator.CreateInstance(type);
        }
    }
}
