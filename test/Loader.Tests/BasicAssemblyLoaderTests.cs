using System;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace Loader.Tests
{
    public class BasicAssemblyLoaderTests
    {
        [Fact]
        public void LoadsNetCoreApp20Project()
        {
            var path = TestResources.GetTestProjectAssembly("NetCoreApp20App");
            var loader = AssemblyLoader.CreateFromFile(path);
            var assembly = loader.LoadDefault();

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
            var loader = AssemblyLoader.CreateFromFile(path);
            var assembly = loader.LoadDefault();

            var type = assembly.GetType("NetStandardClassLib.Class1", throwOnError: true);
            var method = type.GetMethod("GetColor", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal("Red", method.Invoke(Activator.CreateInstance(type), Array.Empty<object>()));
        }
    }
}
