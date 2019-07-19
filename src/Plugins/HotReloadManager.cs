#if FEATURE_UNLOAD
namespace McMaster.NETCore.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Loader;
    using System.Security.Permissions;

    /// <summary>
    /// Wrapper to manage hot reloading, it takes care of all .dlls inside the specified path
    /// including sub directories
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class HotReloadManager<T> where T : class
    {
        private class Plugin
        {
            public string Name { get; }
            public string FullPath { get; }
            public T Module { get; }
            public int LoaderHashCode { get; }

            public Plugin(string name, string fullPath, T module, int loaderHashCode)
            {
                Name = name;
                FullPath = fullPath;
                Module = @module;
                LoaderHashCode = loaderHashCode;
            }
        }

        /// <summary>
        /// Property to access the Plugins
        /// </summary>
        public IReadOnlyDictionary<string, T> Plugins => _plugins.Values.ToDictionary(p => p.Name, p => p.Module);
        private Dictionary<string, Plugin> _plugins;
        private Dictionary<int, PluginLoader> _loaders;
        private FileSystemWatcher _watcher;

        /// <summary>
        /// Creates a instance using the AppContext.BaseDirector + the specified folder name for the plugins
        /// </summary>
        /// <param name="pluginFolderName"></param>
        /// <returns></returns>
        public static HotReloadManager<T> InitializeBasePath(string pluginFolderName) => new HotReloadManager<T>().Configure($@"{AppContext.BaseDirectory}\{pluginFolderName}\");

        /// <summary>
        /// Creates a instance using a complete path for the plugins folder
        /// </summary>
        /// <param name="pluginsDirectory"></param>
        /// <returns></returns>
        public static HotReloadManager<T> InitializeFullPath(string pluginsDirectory) => new HotReloadManager<T>().Configure(pluginsDirectory);

        /// <summary>
        /// Initializes the collections
        /// Configures the watcher to take care of the plugin folder specified
        /// Load the existing plugins at startup
        /// </summary>
        /// <param name="pluginsDirectory"></param>
        /// <returns></returns>
        private HotReloadManager<T> Configure(string pluginsDirectory)
        {
            Directory.CreateDirectory(pluginsDirectory);

            _plugins = new Dictionary<string, Plugin>();
            _loaders = new Dictionary<int, PluginLoader>();

            _watcher = new FileSystemWatcher();
            _watcher.NotifyFilter = NotifyFilters.LastAccess
                                  | NotifyFilters.LastWrite
                                  | NotifyFilters.FileName
                                  | NotifyFilters.DirectoryName;
            _watcher.Created += OnCreated;
            _watcher.Changed += OnChanged;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Path = pluginsDirectory;
            _watcher.EnableRaisingEvents = true;
            _watcher.IncludeSubdirectories = true;

            LoadPluginsFromDirectory(pluginsDirectory);

            return this;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (IsDirectory(e.FullPath))
            {
                LoadPluginsFromDirectory(e.FullPath);
            }
            else
            {
                LoadAssembly(e.FullPath);
            }
        }

        //In case of file replacement the event fires 3 times, this makes sure it will only run once
        int _count = 0;
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (++_count < 3)
            {
                return;
            }

            _count = 0;
            ReloadAssembly(e);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (IsDirectory(e.FullPath))
            {
                RemoveAssembly(e.FullPath);
            }
            else
            {
                RemoveAssembly(e.Name);
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            //TO-DO: Implement the logic to manage file or directory rename
            //It has to update the FullPath of the changed files/directories
        }

        private void LoadPluginsFromDirectory(string pluginsDirectory)
        {
            var plugins = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.AllDirectories);

            foreach (var plugin in plugins)
            {
                LoadAssembly(plugin);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void LoadAssembly(string pluginDllFullPath)
        {
            var fileName = Path.GetFileName(pluginDllFullPath);
            if (File.Exists(pluginDllFullPath))
            {
                var loader = PluginLoader.CreateFromAssemblyFile(pluginDllFullPath, true, sharedTypes: new[] { typeof(T) });

                foreach (var pluginType in loader.LoadDefaultAssembly().GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    var plugin = (T)Activator.CreateInstance(pluginType);
                    _plugins.Add(pluginType.FullName, new Plugin(pluginType.FullName, pluginDllFullPath, plugin, loader.GetHashCode()));
                }

                _loaders.Add(loader.GetHashCode(), loader);
            }
        }

        private void RemoveAssembly(string name)
        {
            var plugins = _plugins.Where(p => p.Value.FullPath.Contains(name)).ToList();
            var loadersHashCodes = plugins.Select(p => p.Value.LoaderHashCode).Distinct();

            foreach (var hashCode in loadersHashCodes)
            {
                _loaders[hashCode].Dispose();

                try
                {
                    _loaders[hashCode].LoadDefaultAssembly();
                    throw new AppDomainUnloadedException(nameof(name));
                }
                catch (ObjectDisposedException)
                {
                    foreach (var plugin in plugins)
                    {
                        _plugins.Remove(plugin.Key);
                    }
                    _loaders.Remove(hashCode);
                }
                catch (AppDomainUnloadedException ex)
                {
                    throw new AppDomainUnloadedException(ex.Message);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private void ReloadAssembly(FileSystemEventArgs path)
        {
            if (!IsDirectory(path.FullPath))
            {
                RemoveAssembly(Path.GetFileName(path.Name));
                LoadAssembly(path.FullPath);
            }     
        }

        private bool IsDirectory(string path)
        {
            FileAttributes attr;

            try
            {
                attr = File.GetAttributes(path);
            }
            catch (Exception)
            {
                attr = FileAttributes.Directory;
            }

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }

            return false;
        }
    }
}
#endif
