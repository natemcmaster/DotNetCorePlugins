// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyModel;
using System.Linq;

namespace McMaster.NETCore.Plugins.Loader
{
    /// <summary>
    /// A cross-platform dependency resolver that reads .deps.json files to resolve assembly and native library paths.
    /// This is an alternative to AssemblyDependencyResolver which is compatible with Android.
    /// Uses Microsoft.Extensions.DependencyModel for parsing .deps.json files.
    /// </summary>
    internal class DepsJsonDependencyResolver
    {
        private readonly string _basePath;
        private readonly Dictionary<string, string> _assemblyPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _nativeLibraryPaths = new(StringComparer.OrdinalIgnoreCase);

        public DepsJsonDependencyResolver(string mainAssemblyPath)
        {
            if (string.IsNullOrEmpty(mainAssemblyPath))
            {
                throw new ArgumentException("Assembly path cannot be null or empty", nameof(mainAssemblyPath));
            }

            _basePath = Path.GetDirectoryName(mainAssemblyPath) ?? throw new ArgumentException("Could not determine base path", nameof(mainAssemblyPath));

            var depsJsonPath = Path.ChangeExtension(mainAssemblyPath, ".deps.json");
            if (File.Exists(depsJsonPath))
            {
                try
                {
                    using var reader = new DependencyContextJsonReader();
                    using var stream = File.OpenRead(depsJsonPath);
                    var dependencyContext = reader.Read(stream);

                    if (dependencyContext != null)
                    {
                        ParseDependencyContext(dependencyContext);
                    }
                }
                catch
                {
                    // If we can't parse the deps.json, we'll fall back to searching directories
                }
            }
        }

        public string? ResolveAssemblyToPath(AssemblyName assemblyName)
        {
            if (assemblyName.Name == null)
            {
                return null;
            }

            if (_assemblyPaths.TryGetValue(assemblyName.Name, out var path))
            {
                return path;
            }

            var dllPath = Path.Combine(_basePath, assemblyName.Name + ".dll");
            if (File.Exists(dllPath))
            {
                return dllPath;
            }

            return null;
        }

        public string? ResolveUnmanagedDllToPath(string unmanagedDllName)
        {
            if (string.IsNullOrEmpty(unmanagedDllName))
            {
                return null;
            }

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(unmanagedDllName);

            if (_nativeLibraryPaths.TryGetValue(unmanagedDllName, out var path))
            {
                return path;
            }

            if (_nativeLibraryPaths.TryGetValue(nameWithoutExtension, out path))
            {
                return path;
            }

            foreach (var prefix in PlatformInformation.NativeLibraryPrefixes)
            {
                var prefixedName = prefix + nameWithoutExtension;
                if (_nativeLibraryPaths.TryGetValue(prefixedName, out path))
                {
                    return path;
                }

                foreach (var ext in PlatformInformation.NativeLibraryExtensions)
                {
                    var fullName = prefixedName + ext;
                    if (_nativeLibraryPaths.TryGetValue(fullName, out path))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        private void ParseDependencyContext(DependencyContext dependencyContext)
        {
            var currentRid = GetRuntimeIdentifier();
            var allNativeAssets = new List<(string assetPath, string? rid, string libraryName)>();

            // Process runtime libraries (managed assemblies and collect native assets)
            foreach (var library in dependencyContext.RuntimeLibraries)
            {
                // Process managed assemblies
                foreach (var assembly in library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths))
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(assembly);
                    var assemblyPath = Path.Combine(_basePath, assembly);

                    if (File.Exists(assemblyPath) && !_assemblyPaths.ContainsKey(assemblyName))
                    {
                        _assemblyPaths[assemblyName] = assemblyPath;
                    }
                }

                // Process runtime-specific managed assemblies
                var runtimeAssemblyGroup = library.RuntimeAssemblyGroups.FirstOrDefault(g =>
                    !string.IsNullOrEmpty(g.Runtime) && (g.Runtime == currentRid || IsCompatibleRid(g.Runtime, currentRid)));

                if (runtimeAssemblyGroup != null)
                {
                    foreach (var assembly in runtimeAssemblyGroup.AssetPaths)
                    {
                        var assemblyName = Path.GetFileNameWithoutExtension(assembly);
                        var assemblyPath = Path.Combine(_basePath, assembly);

                        if (File.Exists(assemblyPath))
                        {
                            _assemblyPaths[assemblyName] = assemblyPath;
                        }
                    }
                }

                // Collect native library assets for later processing
                foreach (var group in library.NativeLibraryGroups)
                {
                    foreach (var asset in group.AssetPaths)
                    {
                        allNativeAssets.Add((asset, group.Runtime, library.Name));
                    }
                }
            }

            // Process all native assets with proper RID prioritization
            ProcessAllNativeAssets(allNativeAssets, currentRid);
        }

        private void ProcessAllNativeAssets(List<(string assetPath, string? rid, string libraryName)> allNativeAssets, string? currentRid)
        {
            // Group by file name to handle duplicates across different RIDs
            var assetsByFileName = new Dictionary<string, List<(string assetPath, string? rid, string libraryName)>>(StringComparer.OrdinalIgnoreCase);

            foreach (var asset in allNativeAssets)
            {
                var fileName = Path.GetFileName(asset.assetPath);
                if (!assetsByFileName.TryGetValue(fileName, out var value))
                {
                    value = [];
                    assetsByFileName[fileName] = value;
                }

                value.Add(asset);
            }

            // Process each unique file name, prioritizing exact RID matches
            foreach (var candidates in assetsByFileName.Select(x => x.Value))
            {
                // Try to find exact match first
                var exactMatch = candidates.FirstOrDefault(c => c.rid == currentRid);
                if (exactMatch != default)
                {
                    RegisterNativeLibrary(exactMatch.assetPath, exactMatch.rid);
                    continue;
                }

                // Try compatible RIDs
                var compatibleMatch = candidates.FirstOrDefault(c =>
                    !string.IsNullOrEmpty(c.rid) && IsCompatibleRid(c.rid, currentRid));
                if (compatibleMatch != default)
                {
                    RegisterNativeLibrary(compatibleMatch.assetPath, compatibleMatch.rid);
                    continue;
                }

                // Fallback to any available
                var fallbackMatch = candidates.FirstOrDefault();
                if (fallbackMatch != default)
                {
                    RegisterNativeLibrary(fallbackMatch.assetPath, fallbackMatch.rid);
                }
            }
        }

        private void RegisterNativeLibrary(string assetPath, string? rid)
        {
            // Try the full path from the deps.json first
            var fullPath = Path.Combine(_basePath, assetPath);
            if (File.Exists(fullPath))
            {
                AddNativeLibraryPath(Path.GetFileName(assetPath), fullPath);
                return;
            }

            // Try RID-specific path
            if (rid != null)
            {
                var fileNameOnly = Path.GetFileName(assetPath);
                var ridSpecificPath = Path.Combine(_basePath, "runtimes", rid, "native", fileNameOnly);
                if (File.Exists(ridSpecificPath))
                {
                    AddNativeLibraryPath(fileNameOnly, ridSpecificPath);
                    return;
                }
            }

            // Fallback to searching
            FallbackRegisterNativeLibrary(assetPath);
        }

        private void FallbackRegisterNativeLibrary(string assetPath)
        {
            var fileName = Path.GetFileName(assetPath);
            var runtimeId = GetRuntimeIdentifier();

            var filePath = Path.Combine(_basePath, fileName);
            if (File.Exists(filePath))
            {
                AddNativeLibraryPath(fileName, filePath);
                return;
            }

            var runtimesDir = Path.Combine(_basePath, "runtimes");
            if (Directory.Exists(runtimesDir) && runtimeId != null)
            {
                var ridSpecificPath = Path.Combine(runtimesDir, runtimeId, "native", fileName);
                if (File.Exists(ridSpecificPath))
                {
                    AddNativeLibraryPath(fileName, ridSpecificPath);
                    return;
                }

                var osOnlyRid = runtimeId.Split('-')[0];
                var osSpecificDirs = Directory.GetDirectories(runtimesDir, osOnlyRid + "-*", SearchOption.TopDirectoryOnly);

                foreach (var osDir in osSpecificDirs)
                {
                    var nativePath = Path.Combine(osDir, "native", fileName);
                    if (File.Exists(nativePath))
                    {
                        AddNativeLibraryPath(fileName, nativePath);
                        return;
                    }
                }
            }
        }

        private void AddNativeLibraryPath(string fileName, string filePath)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            // Only add if not already present, to preserve the first (most specific) match
            if (!_nativeLibraryPaths.ContainsKey(fileName))
            {
                _nativeLibraryPaths[fileName] = filePath;
            }

            if (!_nativeLibraryPaths.ContainsKey(nameWithoutExt))
            {
                _nativeLibraryPaths[nameWithoutExt] = filePath;
            }

            foreach (var prefix in PlatformInformation.NativeLibraryPrefixes.Where(nameWithoutExt.StartsWith))
            {
                var unprefixedName = nameWithoutExt[prefix.Length..];
                if (!_nativeLibraryPaths.ContainsKey(unprefixedName))
                {
                    _nativeLibraryPaths[unprefixedName] = filePath;
                }
            }
        }

        private static string? GetRuntimeIdentifier()
        {
            var os = "";
            var arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = "win";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = "linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = "osx";
            }

            if (string.IsNullOrEmpty(os))
            {
                return null;
            }

            return $"{os}-{arch}";
        }

        /// <summary>
        /// Determines if a given RID is compatible with the current runtime.
        /// This is a simplified version of NuGet's RID graph logic.
        /// 
        /// For full RID compatibility graph support, consider using NuGet.RuntimeModel package.
        /// However, for most scenarios, this simple logic is sufficient:
        /// - Exact matches are always compatible (e.g., win-x64 == win-x64)
        /// - Same OS family is considered compatible (e.g., win-arm64 is compatible with win-x64)
        /// 
        /// Note: This doesn't handle all edge cases like:
        /// - Version-specific RIDs (e.g., win10-x64 vs win-x64)
        /// - Linux distro-specific RIDs (e.g., ubuntu.18.04-x64 vs linux-x64)
        /// </summary>
        /// <param name="rid">The RID to check for compatibility</param>
        /// <param name="currentRid">The current runtime identifier</param>
        /// <returns>True if the RID is compatible with the current runtime</returns>
        private static bool IsCompatibleRid(string rid, string? currentRid)
        {
            if (currentRid == null || string.IsNullOrEmpty(rid))
            {
                return false;
            }

            // Exact match
            if (rid == currentRid)
            {
                return true;
            }

            // Check if the OS part matches (e.g., "win" in "win-x64")
            // This allows cross-architecture compatibility within the same OS family
            var ridParts = rid.Split('-');
            var currentRidParts = currentRid.Split('-');

            if (ridParts.Length > 0 && currentRidParts.Length > 0)
            {
                var ridOs = ridParts[0];
                var currentOs = currentRidParts[0];

                // Handle version-specific OS identifiers (e.g., win10 -> win, ubuntu.18.04 -> ubuntu)
                if (ridOs.Contains('.'))
                {
                    ridOs = ridOs.Split('.')[0];
                }

                if (currentOs.Contains('.'))
                {
                    currentOs = currentOs.Split('.')[0];
                }

                // Remove version numbers from OS (e.g., win10 -> win)
                ridOs = new string([.. ridOs.TakeWhile(c => !char.IsDigit(c))]);
                currentOs = new string([.. currentOs.TakeWhile(c => !char.IsDigit(c))]);

                return ridOs.Equals(currentOs, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
