using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.Extensions.Plugins
{
    public static class DependencyContextExtensions
    {
        public static ManagedLoadContextBuilder TryAddDependencyContext(this ManagedLoadContextBuilder builder, string depsJsonFilePath)
        {
            try
            {
                builder.AddDependencyContext(depsJsonFilePath);
            }
            catch
            { }

            return builder;
        }

        public static ManagedLoadContextBuilder AddDependencyContext(this ManagedLoadContextBuilder builder, string depsJsonFilePath)
        {

            var reader = new DependencyContextJsonReader();
            using (var file = File.OpenRead(depsJsonFilePath))
            {
                var deps = reader.Read(file);
                builder.SetBaseDirectory(Path.GetDirectoryName(depsJsonFilePath));
                builder.AddDependencyContext(deps);
            }

            return builder;
        }

        private static string GetFallbackRid()
        {
            // see https://github.com/dotnet/core-setup/blob/b64f7fffbd14a3517186b9a9d5cc001ab6e5bde6/src/corehost/common/pal.h#L53-L73

            string ridBase;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ridBase = "win10";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ridBase = "linux";

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ridBase = "osx.10.12";
            }
            else
            {
                return "any";
            }

            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    return ridBase + "-x86";
                case Architecture.X64:
                    return ridBase + "-x64";
                case Architecture.Arm:
                    return ridBase + "-arm";
                case Architecture.Arm64:
                    return ridBase + "-arm64";
            }

            return ridBase;
        }

        public static ManagedLoadContextBuilder AddDependencyContext(this ManagedLoadContextBuilder builder, DependencyContext dependencyContext)
        {
            var ridGraph = dependencyContext.RuntimeGraph.Any()
               ? dependencyContext.RuntimeGraph
               : DependencyContext.Default.RuntimeGraph;

            var rid = DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
            var fallbackRid = GetFallbackRid();
            var fallbackGraph = ridGraph.FirstOrDefault(g => g.Runtime == rid)
                ?? ridGraph.FirstOrDefault(g => g.Runtime == fallbackRid)
                ?? new RuntimeFallbacks("any");

            foreach (var managed in dependencyContext.ResolveRuntimeAssemblies(fallbackGraph))
            {
                builder.AddManagedLibrary(managed);
            }

            foreach (var native in dependencyContext.ResolveNativeAssets(fallbackGraph))
            {
                builder.AddNativeLibrary(native);
            }

            return builder;
        }

        private static IEnumerable<ManagedLibrary> ResolveRuntimeAssemblies(this DependencyContext depContext, RuntimeFallbacks runtimeGraph)
        {
            var rids = GetRids(runtimeGraph);
            return from library in depContext.RuntimeLibraries
                   from assetPath in SelectAssets(rids, library.RuntimeAssemblyGroups)
                   select ManagedLibrary.Create(library.Name, library.Version, assetPath);
        }

        private static IEnumerable<NativeLibrary> ResolveNativeAssets(this DependencyContext depContext, RuntimeFallbacks runtimeGraph)
        {
            var rids = GetRids(runtimeGraph);
            return from library in depContext.RuntimeLibraries
                   from assetPath in SelectAssets(rids, library.NativeLibraryGroups)
                       // workaround for System.Native.a being included in the deps.json file for Microsoft.NETCore.App
                   where !assetPath.EndsWith(".a", StringComparison.Ordinal)
                   select NativeLibrary.Create(library.Name, library.Version, assetPath);
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
    }
}
