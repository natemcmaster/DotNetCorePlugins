using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using McMaster.NETCore.Plugins.LibraryModel;

namespace McMaster.NETCore.Plugins.Loader
{
    /// <summary>
    /// An implementation of <see cref="AssemblyLoadContext" /> which attempts to load managed and native
    /// binaries at runtime immitating some of the behaviors of corehost.
    /// </summary>
    internal class ManagedLoadContext : AssemblyLoadContext
    {
        private readonly string _basePath;
        private readonly IReadOnlyDictionary<string, ManagedLibrary> _managedAssemblies;
        private readonly IReadOnlyDictionary<string, NativeLibrary> _nativeLibraries;
        private readonly IReadOnlyCollection<string> _privateAssemblies;
        private readonly IReadOnlyCollection<string> _defaultAssemblies;
        private readonly IReadOnlyCollection<string> _additionalProbingPaths;
        private readonly bool _preferDefaultLoadContext;

        private static readonly string[] s_nativeLibraryExtensions;
        private static readonly string[] s_nativeLibraryPrefixes;
        private static readonly string[] s_managedAssemblyExtensions = new[]
        {
                ".dll",
                ".ni.dll",
                ".exe",
                ".ni.exe"
        };

        static ManagedLoadContext()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                s_nativeLibraryPrefixes = new[] { "" };
                s_nativeLibraryExtensions = new[] { ".dll" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                s_nativeLibraryPrefixes = new[] { "", "lib", };
                s_nativeLibraryExtensions = new[] { ".dylib" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                s_nativeLibraryPrefixes = new[] { "", "lib" };
                s_nativeLibraryExtensions = new[] { ".so", ".so.1" };
            }
            else
            {
                Debug.Fail("Unknown OS type");
                s_nativeLibraryPrefixes = Array.Empty<string>();
                s_nativeLibraryExtensions = Array.Empty<string>();
            }
        }

        public ManagedLoadContext(
            string baseDirectory,
            IReadOnlyDictionary<string, ManagedLibrary> managedAssemblies,
            IReadOnlyDictionary<string, NativeLibrary> nativeLibraries,
            IReadOnlyCollection<string> privateAssemblies,
            IReadOnlyCollection<string> defaultAssemblies,
            IReadOnlyCollection<string> additionalProbingPaths,
            bool preferDefaultLoadContext)
        {
            _basePath = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
            _managedAssemblies = managedAssemblies ?? throw new ArgumentNullException(nameof(managedAssemblies));
            _privateAssemblies = privateAssemblies ?? throw new ArgumentNullException(nameof(privateAssemblies));
            _defaultAssemblies = defaultAssemblies ?? throw new ArgumentNullException(nameof(defaultAssemblies));
            _nativeLibraries = nativeLibraries ?? throw new ArgumentNullException(nameof(nativeLibraries));
            _additionalProbingPaths = additionalProbingPaths ?? throw new ArgumentNullException(nameof(additionalProbingPaths));
            _preferDefaultLoadContext = preferDefaultLoadContext;
        }

        /// <summary>
        /// Load an assembly.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if ((_preferDefaultLoadContext || _defaultAssemblies.Contains(assemblyName.Name)) && !_privateAssemblies.Contains(assemblyName.Name))
            {
                // If default context is preferred, check first for types in the default context unless the dependency has been declared as private
                try
                {
                    var defaultAssembly = Default.LoadFromAssemblyName(assemblyName);
                    if (defaultAssembly != null)
                    {
                        // return null so ALC will fallback to loading from Default ALC directly
                        return null;
                    }
                }
                catch
                {
                    // Swallow errors in loading from the default context
                }
            }

            if (_managedAssemblies.TryGetValue(assemblyName.Name, out var library)
                && SearchForLibrary(library, out var path))
            {
                return LoadFromAssemblyPath(path);
            }

            return null;
        }

        /// <summary>
        /// Loads the unmanaged binary using configured list of native libraries.
        /// </summary>
        /// <param name="unmanagedDllName"></param>
        /// <returns></returns>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            foreach (var prefix in s_nativeLibraryPrefixes)
            {
                if (_nativeLibraries.TryGetValue(prefix + unmanagedDllName, out var library)
                    && SearchForLibrary(library, prefix, out var path))
                {
                    return LoadUnmanagedDllFromPath(path);
                }
            }

            return base.LoadUnmanagedDll(unmanagedDllName);
        }

        private bool SearchForLibrary(ManagedLibrary library, out string path)
        {
            // 1. Search in base path
            foreach (var ext in s_managedAssemblyExtensions)
            {
                var local = Path.Combine(_basePath, library.Name.Name + ext);
                if (File.Exists(local))
                {
                    path = local;
                    return true;
                }
            }

            // 2. Search additional probing paths
            foreach (var searchPath in _additionalProbingPaths)
            {
                var candidate = Path.Combine(searchPath, library.AdditionalProbingPath);
                if (File.Exists(candidate))
                {
                    path = candidate;
                    return true;
                }
            }

            path = null;
            return false;
        }

        private bool SearchForLibrary(NativeLibrary library, string prefix, out string path)
        {
            // 1. Search in base path
            foreach (var ext in s_nativeLibraryExtensions)
            {
                var candidate = Path.Combine(_basePath, $"{prefix}{library.Name}{ext}");
                if (File.Exists(candidate))
                {
                    path = candidate;
                    return true;
                }
            }

            // 2. Search in base path + app local (for portable deployments of netcoreapp)
            var local = Path.Combine(_basePath, library.AppLocalPath);
            if (File.Exists(local))
            {
                path = local;
                return true;
            }

            // 3. Search additional probing paths
            foreach (var searchPath in _additionalProbingPaths)
            {
                var candidate = Path.Combine(searchPath, library.AdditionalProbingPath);
                if (File.Exists(candidate))
                {
                    path = candidate;
                    return true;
                }
            }

            path = null;
            return false;
        }
    }
}
