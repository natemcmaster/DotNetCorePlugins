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

        [SkippableFact]
        [SkipOnOS(OS.Linux | OS.MacOS)]
        public void LoadsNativeDependenciesWhenDllImportUsesFilename()
        {
            // SqlClient has P/invoke that calls "sni.dll" on Windows. This test checks
            // that native libraries can still be resolved in this case.
            var path = TestResources.GetTestProjectAssembly("SqlClientApp");
            var loader = PluginLoader.CreateFromConfigFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var method = assembly
                .GetType("SqlClientApp.Program", throwOnError: true)
                .GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal(true, method.Invoke(null, Array.Empty<object>()));
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
        public void ItPrefersRuntimeSpecificManagedAssetsOverRidlessOnes()
        {
            // System.Drawing.Common is an example of a package which has both rid-specific and ridless versions
            // The package has lib/netstandard2.0/System.Drawing.Common.dll, but also has runtimes/{win,unix}/lib/netcoreapp2.0/System.Drawing.Common.dll
            // In this case, the host will pick the rid-specific version

            var path = TestResources.GetTestProjectAssembly("DrawingApp");
            var loader = PluginLoader.CreateFromConfigFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var type = assembly.GetType("Finder", throwOnError: true);
            var method = type.GetMethod("FindDrawingAssembly", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Contains("runtimes", (string)method.Invoke(null, Array.Empty<object>()));
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
            return (IFruit)Activator.CreateInstance(type);
        }
    }
}
