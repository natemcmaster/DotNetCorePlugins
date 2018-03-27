using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Loader
{
    /// <summary>
    /// A load context that uses a given list of files and locations to load both managed
    /// and unmanaged libraries.
    /// </summary>
    internal class DependencyLoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, string> _assemblyPaths;
        private readonly IReadOnlyDictionary<string, string> _nativeLibraries;
        private readonly IReadOnlyCollection<string> _searchPaths;
        private readonly IReadOnlyDictionary<string, Assembly> _defaultAssemblies;
        private readonly AssemblyName _defaultAssemblyName;
        private static readonly string[] s_nativeLibraryExtensions;
        private static readonly string[] s_managedAssemblyExtensions = new[]
        {
            ".dll",
            ".ni.dll",
            ".exe",
            ".ni.exe"
        };

        static DependencyLoadContext()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                s_nativeLibraryExtensions = new[] { ".dll" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                s_nativeLibraryExtensions = new[] { ".dylib" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                s_nativeLibraryExtensions = new[] { ".so", ".so.1" };
            }
            else
            {
                s_nativeLibraryExtensions = Array.Empty<string>();
            }
        }


        /// <summary>
        /// Initializes a new instance of <see cref="DependencyLoadContext" /> with a predefined
        /// list of paths.
        /// </summary>
        /// <param name="assemblyPaths">Paths to assemblies.</param>
        /// <param name="nativeLibraries">Paths to native, unmanaged libraries.</param>
        /// <param name="searchPaths">Other places to search for files.</param>
        /// <param name="defaultAssemblies">Assemblies to always load from the default context.</param>
        /// <param name="defaultAssemblyName">The default assembly for the load context.</param>
        public DependencyLoadContext(
            IReadOnlyDictionary<string, string> assemblyPaths,
            IReadOnlyDictionary<string, string> nativeLibraries,
            IReadOnlyCollection<string> searchPaths,
            IReadOnlyDictionary<string, Assembly> defaultAssemblies,
            AssemblyName defaultAssemblyName)
        {
            _assemblyPaths = assemblyPaths ?? throw new ArgumentNullException(nameof(assemblyPaths));
            _nativeLibraries = nativeLibraries ?? throw new ArgumentNullException(nameof(nativeLibraries));
            _searchPaths = searchPaths ?? throw new ArgumentNullException(nameof(searchPaths));
            _defaultAssemblies = defaultAssemblies ?? throw new ArgumentNullException(nameof(defaultAssemblies));
            _defaultAssemblyName = defaultAssemblyName;
        }

        public Assembly LoadDefaultAssembly()
        {
            if (_defaultAssemblyName == null)
            {
                throw new InvalidOperationException("This loader does not have a default assembly.");
            }

            return LoadFromAssemblyName(_defaultAssemblyName);
        }

        /// <inheritdoc />
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (_defaultAssemblies.TryGetValue(assemblyName.Name, out var defaultAssembly))
            {
                return defaultAssembly;
            }

            if (_assemblyPaths.TryGetValue(assemblyName.Name, out var path)
                || SearchForLibrary(s_managedAssemblyExtensions, assemblyName.Name, out path))
            {
                return LoadFromAssemblyPath(path);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            if (_nativeLibraries.TryGetValue(unmanagedDllName, out var path)
                || SearchForLibrary(s_nativeLibraryExtensions, unmanagedDllName, out path))
            {
                return LoadUnmanagedDllFromPath(path);
            }

            return base.LoadUnmanagedDll(unmanagedDllName);
        }

        private bool SearchForLibrary(string[] extensions, string name, out string path)
        {
            foreach (var searchPath in _searchPaths)
            {
                foreach (var extension in extensions)
                {
                    var candidate = Path.Combine(searchPath, name + extension);
                    if (File.Exists(candidate))
                    {
                        path = candidate;
                        return true;
                    }
                }
            }
            path = null;
            return false;
        }
    }
}
