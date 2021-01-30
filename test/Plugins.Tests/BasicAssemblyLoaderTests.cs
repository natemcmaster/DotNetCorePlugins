using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using McMaster.Extensions.Xunit;
using Test.Referenced.Library;
using Xunit;

namespace McMaster.NETCore.Plugins.Tests
{
    public class BasicAssemblyLoaderTests
    {
#if NETCOREAPP3_1
        [Fact]
        public void PluginLoaderCanUnload()
        {
            var path = TestResources.GetTestProjectAssembly("NetCoreApp2App");

            // See https://github.com/dotnet/coreclr/pull/22221

            ExecuteAndUnload(path, out var weakRef);

            // Force a GC collect to ensure unloaded has completed
            for (var i = 0; weakRef.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Assert.False(weakRef.IsAlive);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // ensure no local vars are create
        private void ExecuteAndUnload(string path, out WeakReference weakRef)
        {
            var loader = PluginLoader.CreateFromAssemblyFile(path, c => { c.IsUnloadable = true; });
            var assembly = loader.LoadDefaultAssembly();

            var method = assembly
                .GetType("NetCoreApp2App.Program", throwOnError: true)!
                .GetMethod("GetGreeting", BindingFlags.Static | BindingFlags.Public);

            Assert.True(loader.IsUnloadable);
            Assert.NotNull(method);
            Assert.Equal("Hello world!", method!.Invoke(null, Array.Empty<object>()));
            loader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => loader.LoadDefaultAssembly());

            weakRef = new WeakReference(loader.LoadContext, trackResurrection: true);
        }
#endif

        [Fact]
        public void LoadsNetCoreProjectWithNativeDeps()
        {
            var path = TestResources.GetTestProjectAssembly("PowerShellPlugin");
            var loader = PluginLoader.CreateFromAssemblyFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var method = assembly
                .GetType("PowerShellPlugin.Program", throwOnError: true)!
                .GetMethod("GetGreeting", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal("hello", method!.Invoke(null, Array.Empty<object>()));
        }

        [SkippableFact]
        [SkipOnOS(OS.Linux | OS.MacOS)]
        public void LoadsNativeDependenciesWhenDllImportUsesFilename()
        {
            // SqlClient has P/invoke that calls "sni.dll" on Windows. This test checks
            // that native libraries can still be resolved in this case.
            var path = TestResources.GetTestProjectAssembly("SqlClientApp");
            var loader = PluginLoader.CreateFromAssemblyFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var method = assembly
                .GetType("SqlClientApp.Program", throwOnError: true)!
                .GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal(true, method!.Invoke(null, Array.Empty<object>()));
        }

        [Fact]
        public void LoadsNetCoreApp2Project()
        {
            var path = TestResources.GetTestProjectAssembly("NetCoreApp2App");
            var loader = PluginLoader.CreateFromAssemblyFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var method = assembly
                .GetType("NetCoreApp2App.Program", throwOnError: true)!
                .GetMethod("GetGreeting", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal("Hello world!", method!.Invoke(null, Array.Empty<object>()));
        }

        [Fact]
        public void LoadsNetStandard20Project()
        {
            var path = TestResources.GetTestProjectAssembly("NetStandardClassLib");
            var loader = PluginLoader.CreateFromAssemblyFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var type = assembly.GetType("NetStandardClassLib.Class1", throwOnError: true);
            var method = type!.GetMethod("GetColor", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Equal("Red", method!.Invoke(Activator.CreateInstance(type), Array.Empty<object>()));
        }

        [Fact]
        public void ItPrefersRuntimeSpecificManagedAssetsOverRidlessOnes()
        {
            // System.Drawing.Common is an example of a package which has both rid-specific and ridless versions
            // The package has lib/netstandard2.0/System.Drawing.Common.dll, but also has runtimes/{win,unix}/lib/netcoreapp2.0/System.Drawing.Common.dll
            // In this case, the host will pick the rid-specific version

            var path = TestResources.GetTestProjectAssembly("DrawingApp");
            var loader = PluginLoader.CreateFromAssemblyFile(path);
            var assembly = loader.LoadDefaultAssembly();

            var type = assembly.GetType("Finder", throwOnError: true)!;
            var method = type.GetMethod("FindDrawingAssembly", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.Contains("runtimes", (string?)method!.Invoke(null, Array.Empty<object>()));
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
            var loader = PluginLoader.CreateFromAssemblyFile(path,
#if NETCOREAPP3_1
                isUnloadable: true,
#endif
                sharedTypes: new[] { typeof(IFruit) });

            var assembly = loader.LoadDefaultAssembly();
            var type = Assert.Single(assembly.GetTypes(), t => typeof(IFruit).IsAssignableFrom(t));
            return (IFruit)Activator.CreateInstance(type)!;
        }
    }
}
