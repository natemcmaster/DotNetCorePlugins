using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;

namespace McMaster.NETCore.Plugins.Manager
{
    /// <summary>
    /// <para>Takes care of all assemblies inside the specified path including subdirectories.</para>
    /// <para>All the TType classes (modules) will be loaded to Modules or InactiveModules depending on configuration.</para>
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public sealed partial class ModuleManager<TType> where TType : class
    {
        #region --- Properties ---
        private readonly Dictionary<string, TType> _modules;
        private readonly Dictionary<string, TType> _inactiveModules;
        private readonly ModuleManagerConfiguration _configurations;
#if FEATURE_UNLOAD
        private readonly Dictionary<string, Plugin> _plugins;
#endif
        private readonly Dictionary<int, PluginLoader> _loaders;
        private readonly object[] _params;

        //───────────────────────────────────────

        /// <summary>
        /// Active modules that are ready to be used by the client.
        /// </summary>
        public IReadOnlyDictionary<string, TType> Modules => _modules;
        /// <summary>
        /// <para>Inactive modules that require manual activation.</para>
        /// <para>Their purpose is to support loading modules but keep them away from the front-end while some business rule is not met.</para>
        /// <para>Example: A module should have the state active in a database.</para>
        /// <para>All the validations should be done by the client and when met activate them with managerInstance.ActivateModule(name)</para>
        /// </summary>
        public IReadOnlyDictionary<string, TType> InactiveModules => _inactiveModules;
#endregion

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

#region --- Events Properties ---
        private ModuleEventHandler<TType> _onLoadHandler = null;
#if FEATURE_UNLOAD
        private UnloadEventHandler _onUnloadHandler = null;
#endif

        //───────────────────────────────────────

        private ModuleEventHandler<TType> _onInactiveLoadHandler = null;
#if FEATURE_UNLOAD
        private UnloadEventHandler _onInactiveUnloadHandler = null;

        //───────────────────────────────────────

        private ErrorEventHandler _onErrorHandler = null;
#endif

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        /// <summary>
        /// Occurs when a module is load as active.
        /// </summary>
        public event ModuleEventHandler<TType> ModuleLoaded
        {
            add => _onLoadHandler += value;
            remove => _onLoadHandler -= value;
        }

#if FEATURE_UNLOAD
        /// <summary>
        /// Occurs when a active module is unloaded.
        /// </summary>
        public event UnloadEventHandler ModuleUnLoaded
        {
            add => _onUnloadHandler += value;
            remove => _onUnloadHandler -= value;
        }
#endif

        //───────────────────────────────────────
        
        /// <summary>
        /// Occurs when a module is load as inactive.
        /// </summary>
        public event ModuleEventHandler<TType> InactiveModuleLoaded
        {
            add => _onInactiveLoadHandler += value;
            remove => _onInactiveLoadHandler -= value;
        }

#if FEATURE_UNLOAD
        /// <summary>
        /// Occurs when a inactive module could not be unloaded.
        /// </summary>
        public event UnloadEventHandler InactiveModuleUnLoaded
        {
            add => _onInactiveUnloadHandler += value;
            remove => _onInactiveUnloadHandler -= value;
        }

        //───────────────────────────────────────

        /// <summary>
        /// Occurs when a module could not be unloaded.
        /// </summary>
        public event ErrorEventHandler UnloadFailled
        {
            add => _onErrorHandler += value;
            remove => _onErrorHandler -= value;
        }
#endif
#endregion

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        /// <summary>
        /// Creates the ModuleManager
        /// </summary>
        /// <param name="configurations"></param>
        /// <param name="list">Parameters to be passed to the module</param>
        /// <returns></returns>
        public ModuleManager(ModuleManagerConfiguration configurations, params object[] list)
        {
            _configurations = configurations;
            Directory.CreateDirectory(configurations.ModuleFolderPath);
            _params = list;

            _modules = new Dictionary<string, TType>();
            _inactiveModules = new Dictionary<string, TType>();
#if FEATURE_UNLOAD
            _plugins = new Dictionary<string, Plugin>();
#endif
            _loaders = new Dictionary<int, PluginLoader>();

            var watcher = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.FileName
                               | NotifyFilters.DirectoryName
            };
            watcher.Created += OnCreated;
#if FEATURE_UNLOAD
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
#endif
            watcher.Renamed += OnRenamed;
            watcher.Path = configurations.ModuleFolderPath;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            LoadPluginsFromDirectory(configurations.ModuleFolderPath);

            _configurations.Startup = false;
            _configurations.ModulesToLoadAtStartup = null;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

#region --- FileWatcher Events ---
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

        //═════════════════════════════════════════════════════════════════════════════════════════

#if FEATURE_UNLOAD
        //In case of file replacement the event fires 3 times, this makes sure it will only run once
        int _count = 0;
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (++_count == 3)
            {
                _count = 0;
                ReloadAssembly(e);
            }
        }
        //═════════════════════════════════════════════════════════════════════════════════════════

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
#endif

        //═════════════════════════════════════════════════════════════════════════════════════════

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            //TODO: Implement the logic to manage file or directory rename
            //It has to update the FullPath of the changed files/directories
        }
#endregion

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
        
#region --- Assemblies Resolver ---
        private void LoadPluginsFromDirectory(string pluginsDirectory)
        {
            var plugins = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.AllDirectories);

            foreach (var plugin in plugins)
            {
                LoadAssembly(plugin);
            }
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void LoadAssembly(string pluginDllFullPath)
        {
            if (File.Exists(pluginDllFullPath))
            {
#if FEATURE_UNLOAD
                var loader = PluginLoader.CreateFromAssemblyFile(pluginDllFullPath, _configurations.HotReload, sharedTypes: new[] { typeof(TType) });
#else
                var loader = PluginLoader.CreateFromAssemblyFile(pluginDllFullPath, sharedTypes: new[] { typeof(TType) });
#endif

                foreach (var pluginType in loader.LoadDefaultAssembly().GetTypes().Where(t => typeof(TType).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    var plugin = (TType)Activator.CreateInstance(pluginType, _params);
#if FEATURE_UNLOAD
                    _plugins.Add(pluginType.Name, new Plugin(pluginDllFullPath, loader.GetHashCode()));
#endif

                    if ((_configurations.Startup || !_configurations.OnLoadSetInactive) && _configurations.ModulesToLoadAtStartup?.Count > 0
                                                                                        && _configurations.ModulesToLoadAtStartup.Contains(pluginType.Name))
                    {
                        _modules.Add(pluginType.Name, plugin);
                        OnLoad(new ModuleEventArgs<TType>(pluginType.Name, plugin));
                        continue;
                    }

                    _inactiveModules.Add(pluginType.Name, plugin);
                    OnInactiveLoad(new ModuleEventArgs<TType>(pluginType.Name, plugin));                    
                }

                _loaders.Add(loader.GetHashCode(), loader);
            }
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

#if FEATURE_UNLOAD
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
                    OnError(new ErrorEventArgs(new AppDomainUnloadedException(nameof(name))));
                }
                catch (ObjectDisposedException)
                {
                    foreach (var plugin in plugins)
                    {
                        _plugins.Remove(plugin.Key);

                        if (_modules.ContainsKey(plugin.Key))
                        {
                            _modules.Remove(plugin.Key);
                            OnUnload(new UnloadEventArgs(plugin.Key));
                        }
                        if (_inactiveModules.ContainsKey(plugin.Key))
                        {
                            _inactiveModules.Remove(plugin.Key);
                            OnInactiveUnload(new UnloadEventArgs(plugin.Key));
                        }
                    }
                    _loaders.Remove(hashCode);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

        private void ReloadAssembly(FileSystemEventArgs path)
        {
            if (!IsDirectory(path.FullPath))
            {
                RemoveAssembly(Path.GetFileName(path.Name));
                LoadAssembly(path.FullPath);
            }
        }
#endif

        //═════════════════════════════════════════════════════════════════════════════════════════

        private static bool IsDirectory(string path)
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

            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }
#endregion

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

#region --- Change Module State ---
        /// <summary>
        /// <para>Changes the module from Inactive to Active</para>
        /// <para>Removes from InactiveModules and adds to Modules</para>
        /// </summary>
        /// <param name="moduleName"></param>
        public void ActivateModule(string moduleName)
        {
            _modules.Add(moduleName, _inactiveModules[moduleName]);
            _inactiveModules.Remove(moduleName);
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// <para>Changes the module from Inactive to Active</para>
        /// <para>Removes from Modules and adds to InactiveModules</para>
        /// </summary>
        /// <param name="moduleName"></param>
        public void DeactivateModule(string moduleName)
        {
            _inactiveModules.Add(moduleName, _modules[moduleName]);
            _modules.Remove(moduleName);
        }
#endregion

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

#region --- Events ---
        private void OnLoad(ModuleEventArgs<TType> e)
            => InvokeOnLoad(e, _onLoadHandler);

#if FEATURE_UNLOAD
        private void OnUnload(UnloadEventArgs e)
            => InvokeOnUnload(e, _onUnloadHandler);
#endif

        //───────────────────────────────────────

        private void OnInactiveLoad(ModuleEventArgs<TType> e)
            => InvokeOnLoad(e, _onInactiveLoadHandler);

#if FEATURE_UNLOAD
        private void OnInactiveUnload(UnloadEventArgs e)
            => InvokeOnUnload(e, _onInactiveUnloadHandler);
#endif

        //───────────────────────────────────────

        private void InvokeOnLoad(ModuleEventArgs<TType> e, ModuleEventHandler<TType> handler)
            => handler?.Invoke(this, e);

#if FEATURE_UNLOAD
        private void InvokeOnUnload(UnloadEventArgs e, UnloadEventHandler handler)
            => handler?.Invoke(this, e);

        //───────────────────────────────────────

        private void OnError(ErrorEventArgs e)
            => _onErrorHandler?.Invoke(this, e);
#endif
#endregion
    }
}
