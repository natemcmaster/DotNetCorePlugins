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
    public class DependencyLoadContext : AssemblyLoadContext
    {
        private readonly IDictionary<string, string> _assemblyPaths;
        private readonly IDictionary<string, string> _nativeLibraries;
        private readonly ICollection<string> _searchPaths;

        private static readonly string[] NativeLibraryExtensions;
        private static readonly string[] ManagedAssemblyExtensions = new[]
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
                NativeLibraryExtensions = new[] { ".dll" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                NativeLibraryExtensions = new[] { ".dylib" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                NativeLibraryExtensions = new[] { ".so" };
            }
            else
            {
                NativeLibraryExtensions = Array.Empty<string>();
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DependencyLoadContext" /> with a predefined
        /// list of paths.
        /// </summary>
        /// <param name="assemblyPaths">Paths to assemblies.</param>
        /// <param name="nativeLibraries">Paths to native, unmanaged libraries.</param>
        /// <param name="searchPaths">Other places to search for files.</param>
        public DependencyLoadContext(
            IDictionary<string, string> assemblyPaths,
            IDictionary<string, string> nativeLibraries,
            ICollection<string> searchPaths)
        {
            _assemblyPaths = assemblyPaths ?? throw new ArgumentNullException(nameof(assemblyPaths));
            _nativeLibraries = nativeLibraries ?? throw new ArgumentNullException(nameof(nativeLibraries));
            _searchPaths = searchPaths ?? throw new ArgumentNullException(nameof(searchPaths));
        }

        /// <summary>
        /// Add the <paramref name="path"/> that should be used to load an assembly with a given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The assembly name</param>
        /// <param name="path">The file path</param>
        public void AddManagedLibrary(AssemblyName name, string path)
        {
            ValidateNameAndPath(name.Name, path);

            _assemblyPaths.Add(name.Name, path);
        }

        /// <summary>
        /// Add the <paramref name="path"/> that should be used to load an native, unmanaged library with a given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The p/invoke name</param>
        /// <param name="path">The file path</param>
        public void AddUnmanagedLibrary(string name, string path)
        {
            ValidateNameAndPath(name, path);

            _nativeLibraries.Add(name, path);
        }

        /// <summary>
        /// Add a <paramref name="path"/> that should be used to search for native and managed libraries.
        /// </summary>
        /// <param name="path">The file path. Must be a full file path.</param>
        public void AddSearchPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(Strings.Error_NullOrEmpty, nameof(path));
            }

            if (!Path.IsPathRooted(path))
            {
                throw new ArgumentException("Argument must be a full path.", nameof(path));
            }

            _searchPaths.Add(path);
        }

        private static void ValidateNameAndPath(string name, string path)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Strings.Error_NullOrEmpty, nameof(name));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(Strings.Error_NullOrEmpty, nameof(path));
            }

            if (!Path.IsPathRooted(path))
            {
                throw new ArgumentException("Argument must be a full path.", nameof(path));
            }
        }

        /// <inheritdoc />
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (_assemblyPaths.TryGetValue(assemblyName.Name, out var path)
                || SearchForLibrary(ManagedAssemblyExtensions, assemblyName.Name, out path))
            {
                return LoadFromAssemblyPath(path);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            if (_nativeLibraries.TryGetValue(unmanagedDllName, out var path)
                || SearchForLibrary(NativeLibraryExtensions, unmanagedDllName, out path))
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
