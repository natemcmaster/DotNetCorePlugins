﻿//#if FEATURE_UNLOAD
namespace McMaster.NETCore.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;

    /// <summary>
    /// 
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

            public override bool Equals(Object obj)
            {
                if (obj == null || !(obj is Plugin))
                {
                    return false;
                }
                else
                {
                    return Name == ((Plugin)obj).Name && FullPath == ((Plugin)obj).FullPath;
                }
            }

            public override int GetHashCode()
            {
                return Tuple.Create(Name, FullPath).GetHashCode();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyCollection<T> Plugins => _plugins.Select(p => p.Module).ToList();
        private HashSet<Plugin> _plugins;
        private Dictionary<int, PluginLoader> _loaders;
        private FileSystemWatcher _watcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginFolderName"></param>
        /// <returns></returns>
        public static HotReloadManager<T> InitializeBasePath(string pluginFolderName) => new HotReloadManager<T>().Configure($@"{AppContext.BaseDirectory}\{pluginFolderName}\");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginsDirectory"></param>
        /// <returns></returns>
        public static HotReloadManager<T> InitializeFullPath(string pluginsDirectory) => new HotReloadManager<T>().Configure(pluginsDirectory);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginsDirectory"></param>
        /// <returns></returns>
        private HotReloadManager<T> Configure(string pluginsDirectory)
        {
            _plugins = new HashSet<Plugin>();
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
            _watcher.Path = pluginsDirectory ?? AppContext.BaseDirectory;
            _watcher.EnableRaisingEvents = true;
            _watcher.IncludeSubdirectories = true;

            LoadPluginsFromDirectory(pluginsDirectory);

            return this;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            var attr = File.GetAttributes(e.FullPath);

            //Detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
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

            ReloadAssembly(e);
            _count = 0;
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            FileAttributes attr;

            try
            {
                attr = File.GetAttributes(e.FullPath);
            }
            catch (Exception)
            {
                attr = FileAttributes.Directory;
            }

            //Detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var plugins = _plugins.Where(p => p.FullPath.Contains(e.FullPath));

                 RemoveAssembly(plugins.FirstOrDefault().FullPath);
            }
            else
            {
                RemoveAssembly(e.Name);
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LoadPluginsFromDirectory(string pluginsDirectory)
        {
            var plugins = System.IO.Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.AllDirectories);

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
                var loader = PluginLoader.CreateFromAssemblyFile(pluginDllFullPath, sharedTypes: new[] { typeof(T) }); //FALTA O TRUE

                foreach (var pluginType in loader.LoadDefaultAssembly().GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    var plugin = (T)Activator.CreateInstance(pluginType);
                    _plugins.Add(new Plugin(pluginType.FullName, pluginDllFullPath, plugin, loader.GetHashCode()));
                }

                _loaders.Add(loader.GetHashCode(), loader);
            }
        }

        private void RemoveAssembly(string name)
        {
            var plugins = _plugins.Where(p => p.FullPath.Contains(name)).ToList();
            var loadersHashCode = plugins.Select(p => p.LoaderHashCode).Distinct();

            _plugins.ExceptWith(plugins);

            foreach (var hashCode in loadersHashCode)
            {
                _loaders[hashCode].Dispose();
                _loaders.Remove(hashCode);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private void ReloadAssembly(FileSystemEventArgs path)
        {
            var attr = File.GetAttributes(path.FullPath);

            //Detect whether its a directory or file
            if (!((attr & FileAttributes.Directory) == FileAttributes.Directory))
            {
                RemoveAssembly(Path.GetFileName(path.Name));
                LoadAssembly(path.FullPath);
            }      
        }
    }
}
//#endif
