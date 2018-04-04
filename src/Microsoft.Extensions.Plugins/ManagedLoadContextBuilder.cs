using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Extensions.Plugins
{
    public class ManagedLoadContextBuilder
    {
        private readonly List<string> _additionalProbingPaths = new List<string>();
        private readonly Dictionary<string, ManagedLibrary> _managedLibraries = new Dictionary<string, ManagedLibrary>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeLibrary> _nativeLibraries = new Dictionary<string, NativeLibrary>(StringComparer.Ordinal);
        private readonly HashSet<string> _privateAssemblies = new HashSet<string>(StringComparer.Ordinal);
        private string _basePath;
        private bool _preferDefaultLoadContext;

        public ManagedLoadContext Build()
        {
            return new ManagedLoadContext(
                _basePath,
                _managedLibraries,
                _nativeLibraries,
                _privateAssemblies,
                _additionalProbingPaths,
                _preferDefaultLoadContext);
        }

        public ManagedLoadContextBuilder SetBaseDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(Strings.Error_NullOrEmpty, nameof(path));
            }

            if (!Path.IsPathRooted(path))
            {
                throw new ArgumentException("Argument must be a full path.", nameof(path));
            }

            _basePath = path;
            return this;
        }

        public ManagedLoadContextBuilder PreferLoadContextAssembly(AssemblyName assemblyName)
        {
            _privateAssemblies.Add(assemblyName.Name);
            return this;
        }

        public ManagedLoadContextBuilder PreferDefaultLoadContext(bool preferDefaultLoadContext)
        {
            _preferDefaultLoadContext = preferDefaultLoadContext;
            return this;
        }

        public ManagedLoadContextBuilder AddManagedLibrary(ManagedLibrary library)
        {
            ValidateRelativePath(library.AdditionalProbingPath);

            _managedLibraries.Add(library.Name.Name, library);
            return this;
        }

        public ManagedLoadContextBuilder AddNativeLibrary(NativeLibrary library)
        {
            ValidateRelativePath(library.AppLocalPath);
            ValidateRelativePath(library.AdditionalProbingPath);

            _nativeLibraries.Add(library.Name, library);
            return this;
        }

        /// <summary>
        /// Add a <paramref name="path"/> that should be used to search for native and managed libraries.
        /// </summary>
        /// <param name="path">The file path. Must be a full file path.</param>
        public ManagedLoadContextBuilder AddProbingPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(Strings.Error_NullOrEmpty, nameof(path));
            }

            if (!Path.IsPathRooted(path))
            {
                throw new ArgumentException("Argument must be a full path.", nameof(path));
            }

            _additionalProbingPaths.Add(path);
            return this;
        }

        private static void ValidateRelativePath(string probingPath)
        {
            if (string.IsNullOrEmpty(probingPath))
            {
                throw new ArgumentException(Strings.Error_NullOrEmpty, nameof(probingPath));
            }

            if (Path.IsPathRooted(probingPath))
            {
                throw new ArgumentException("Argument must be a relative path.", nameof(probingPath));
            }
        }
    }
}
