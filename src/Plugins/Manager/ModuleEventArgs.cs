using System;

namespace McMaster.NETCore.Plugins.Manager
{
    /// <summary>
    /// Provides data to the loaded events.
    /// </summary>
    /// <typeparam name="TModule"></typeparam>
    public class ModuleEventArgs<TModule> : EventArgs
    {
        /// <summary>
        /// Gets the loaded module name.
        /// </summary>
        public string ModuleName { get; private set; }
        /// <summary>
        /// Reference for the actual module object
        /// </summary>
        public TModule Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="module"></param>
        public ModuleEventArgs(string moduleName, TModule module)
        {
            ModuleName = moduleName;
            Value = module;
        }
    }
}
