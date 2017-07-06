using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.DotNet.PlatformAbstractions;

namespace Microsoft.Extensions.DependencyModel
{
    /// <summary>
    /// Helper methods for creating a load context.
    /// </summary>
    public static class DependencyLoadContextExtensions
    {
        static DependencyLoadContextExtensions()
        {
            var envVar = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (!string.IsNullOrEmpty(envVar))
            {
                DefaultNuGetFolder = envVar;
            }
            else
            {
                var home = Environment.GetEnvironmentVariable("USERPROFILE")
                    ?? Environment.GetEnvironmentVariable("HOME")
                    ?? Environment.GetEnvironmentVariable("HOMEDRIVE");

                DefaultNuGetFolder = Path.Combine(home, ".nuget", "packages");
            }
        }

        /// <summary>
        /// Creates a load context given a <paramref name="dependencyContext"/>.
        /// </summary>
        /// <param name="dependencyContext">The dependency context.</param>
        /// <returns>A load context.</returns>
        public static DependencyLoadContext CreateLoadContext(this DependencyContext dependencyContext)
            => CreateLoadContext(dependencyContext, AppContext.BaseDirectory, DefaultNuGetFolder);

        /// <summary>
        /// Creates a load context that immitates corehost
        /// <see href="https://github.com/dotnet/cli/blob/rel/1.0.0/Documentation/specs/corehost.md" />.
        /// </summary>
        /// <param name="dependencyContext">The dependency context</param>
        /// <param name="appBaseDirectory">The application base directory.</param>
        /// <param name="nugetCacheDirectory">The directory containing the NuGet cache.</param>
        /// <returns>The load context</returns>
        public static DependencyLoadContext CreateLoadContext(
            this DependencyContext dependencyContext,
            string appBaseDirectory,
            string nugetCacheDirectory)
        {
            // see https://github.com/dotnet/cli/blob/rel/1.0.0/Documentation/specs/corehost.md
            // 1. servicing cache
            // 2. app-local
            // 3. nuget cache(s)

            var ridGraph = dependencyContext.RuntimeGraph.Any()
                ? dependencyContext.RuntimeGraph
                : DependencyContext.Default.RuntimeGraph;

            var fallbackGraph = ridGraph
                .FirstOrDefault(g => g.Runtime == RuntimeEnvironment.GetRuntimeIdentifier())
                ?? new RuntimeFallbacks("any");

            var searchPaths = new[]
            {
                appBaseDirectory
            };

            // TODO servicing cache
            // TODO runtime store
            // TODO nuget fallback folders
            var managedLibraries = dependencyContext
                .ResolveRuntimeAssemblies(nugetCacheDirectory, fallbackGraph)
                .ToDictionary(r => r.Name, r => r.Path);

            var nativeLibraries = dependencyContext
                .ResolveNativeAssets(nugetCacheDirectory, fallbackGraph)
                .ToDictionary(n => n.Name, n => n.Path);

            return new DependencyLoadContext(managedLibraries, nativeLibraries, searchPaths);
        }

        private static string DefaultNuGetFolder { get; }

        private static IEnumerable<Asset> ResolveRuntimeAssemblies(this DependencyContext depContext, string packageDir, RuntimeFallbacks runtimeGraph)
        {
            var rids = GetRids(runtimeGraph);
            return from library in depContext.RuntimeLibraries
                   from assetPath in SelectAssets(rids, library.RuntimeAssemblyGroups)
                   select Asset.Create(packageDir, library.Name, library.Version, assetPath);
        }

        private static IEnumerable<Asset> ResolveNativeAssets(this DependencyContext depContext,
            string packageDir,
            RuntimeFallbacks runtimeGraph)
        {
            var rids = GetRids(runtimeGraph);
            return from library in depContext.RuntimeLibraries
                   from assetPath in SelectAssets(rids, library.NativeLibraryGroups)
                   // workaround for System.Native.a being included in the deps.json file for Microsoft.NETCore.App
                   where !assetPath.EndsWith(".a", StringComparison.Ordinal)
                   select Asset.Create(packageDir, library.Name, library.Version, assetPath);
        }

        private static IEnumerable<string> GetRids(RuntimeFallbacks runtimeGraph)
        {
            return Enumerable.Concat(new[] { runtimeGraph.Runtime }, runtimeGraph?.Fallbacks ?? Enumerable.Empty<string>());
        }

        private static IEnumerable<string> SelectAssets(IEnumerable<string> rids, IEnumerable<RuntimeAssetGroup> groups)
        {
            foreach (var rid in rids)
            {
                var group = groups.FirstOrDefault(g => g.Runtime == rid);
                if (group != null)
                {
                    return group.AssetPaths;
                }
            }

            // Return the RID-agnostic group
            return groups.GetDefaultAssets();
        }

        private class Asset
        {
            public Asset(string name, string path)
            {
                Name = name;
                Path = path;
            }

            public string Name { get; }
            public string Path { get; }

            public static Asset Create(string packageDir, string libraryName, string version, string path)
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                return new Asset(name, System.IO.Path.Combine(packageDir, libraryName, version, path));
            }
        }
    }
}
