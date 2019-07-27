using System.Collections.Generic;

namespace McMaster.NETCore.Plugins.Manager
{
    /// <summary>
    /// ModuleManager configurations.
    /// </summary>
    public class ModuleManagerConfiguration
    {
        /// <summary>
        /// <para>The path for the folder where the modules are.</para>
        /// <para>Can be either relative or absolute.</para>
        /// </summary>
        public string Path { get; set; }

#if FEATURE_UNLOAD
        /// <summary>
        /// Assemblies will be loaded in-memory so they can be changed/removed at runtime.
        /// </summary>
        public bool HotReload { get; set; }
#endif

        /// <summary>
        /// <para>This option will force the modules (class instances of the Interface provided) to be activated manually via ManagerInstance.ActivateModule(key);</para>
        /// <para>Modules will be added to the InactiveModules dictionary;</para>
        /// <para>This will only affect the modules loaded after the first load so the initial modules can be loaded automatically instead of manually.</para>
        /// </summary>
        public bool OnLoadSetInactive { get; set; }

        /// <summary>
        /// <para>Allows to specify which modules need to be activated manually at startup;</para>
        /// <para>Those modules will be added to the InactiveModules dictionary.</para>
        /// </summary>
        public List<string> ModulesToLoadAtStartup { get; set; }


        internal bool Startup = true;
    }
}
