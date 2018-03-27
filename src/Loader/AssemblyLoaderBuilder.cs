using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace System.Runtime.Loader
{
    public class AssemblyLoaderBuilder
    {
        private readonly List<string> _searchPaths = new List<string>();
        private readonly Dictionary<string, string> _managedAssemblies = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _nativeLibraries = new Dictionary<string, string>();
        private readonly Dictionary<string, Assembly> _defaultAssemblies = new Dictionary<string, Assembly>();
        private AssemblyName _defaultName;

        public AssemblyLoaderBuilder SetDefaultAssemblyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Strings.Error_NullOrEmpty, nameof(name));
            }

            _defaultName = new AssemblyName(name);
            return this;
        }

        public AssemblyLoader Build()
        {
            var context = new DependencyLoadContext(
                _managedAssemblies,
                _nativeLibraries,
                _searchPaths,
                _defaultAssemblies,
                _defaultName);
            return new AssemblyLoader(context);
        }

        public AssemblyLoaderBuilder AddManagedLibrary(string name, string path)
            => AddManagedLibrary(new AssemblyName(name), path);

        /// <summary>
        /// Add the <paramref name="path"/> that should be used to load an assembly with a given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The assembly name</param>
        /// <param name="path">The file path</param>
        public AssemblyLoaderBuilder AddManagedLibrary(AssemblyName name, string path)
        {
            ValidateNameAndPath(name.Name, path);

            _managedAssemblies.Add(name.Name, path);
            return this;
        }

        /// <summary>
        /// Add the <paramref name="path"/> that should be used to load an native, unmanaged library with a given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The p/invoke name</param>
        /// <param name="path">The file path</param>
        public AssemblyLoaderBuilder AddNativeLibrary(string name, string path)
        {
            ValidateNameAndPath(name, path);

            _nativeLibraries.Add(name, path);
            return this;
        }

        /// <summary>
        /// Add a <paramref name="path"/> that should be used to search for native and managed libraries.
        /// </summary>
        /// <param name="path">The file path. Must be a full file path.</param>
        public AssemblyLoaderBuilder AddSearchPath(string path)
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
            return this;
        }


        public AssemblyLoaderBuilder UnifyWith(Assembly assembly)
        {
            _defaultAssemblies.Add(assembly.GetName().Name, assembly);
            return this;
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
    }
}
