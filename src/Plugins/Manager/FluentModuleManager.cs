using System.Collections.Generic;

namespace McMaster.NETCore.Plugins.Manager
{
    /// <summary>
    /// Fluent Extension to help configure the ModuleManager
    /// </summary>
    public class FluentModuleManager<TType> where TType : class
    {
        private readonly ModuleManagerConfiguration _configuration;
        private object[] _parameters;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private FluentModuleManager(string path)
        {
            _configuration = new ModuleManagerConfiguration(path);
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        /// <summary>
        /// Command to start configuring the ModuleManager
        /// </summary>
        /// <returns></returns>
        public static FluentModuleManager<TType> WithPath(string path) => new FluentModuleManager<TType>(path);

        //═════════════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Assemblies will be loaded in-memory so they can be added/changed/removed at runtime.
        /// </summary>
        public FluentModuleManager<TType> WithModuleParameters(params object[] parameters)
        {
            _parameters = parameters;
            return this;
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

#if FEATURE_UNLOAD
        /// <summary>
        /// Assemblies will be loaded in-memory so they can be changed/removed at runtime.
        /// </summary>
        public FluentModuleManager<TType> EnableHotReload()
        {
            _configuration.HotReload = true;
            return this;
        }
#endif

        //═════════════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// <para>This option will force the modules to be activated manually via ManagerInstance.ActivateModule(moduleName);</para>
        /// <para>Modules will be added to the InactiveModules dictionary;</para>
        /// <para>This will only affect the modules loaded after the first load so the initial modules can be loaded automatically instead of manually.</para>
        /// </summary>
        public FluentModuleManager<TType> EnableOnLoadSetInactive()
        {
            _configuration.OnLoadSetInactive = true;
            return this;
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// <para>Allows to specify which modules will be loaded automatically at startup;</para>
        /// <para>All other modules will be loaded as Inactive and requires manual activation.</para>
        /// </summary>
        public FluentModuleManager<TType> SetModulesToLoadAtStartup(List<string> names)
        {
            _configuration.ModulesToLoadAtStartup = names;
            return this;
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Completes the ModuleManager configuration.
        /// </summary>
        public ModuleManager<TType> Complete()
        {
            return new ModuleManager<TType>(_configuration, _parameters);
        }
    }
}
