#if FEATURE_UNLOAD
using System;

namespace McMaster.NETCore.Plugins.Manager
{
    /// <summary>
    /// Provides data to the unloaded events.
    /// </summary>
    public class UnloadEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the unloaded module name.
        /// </summary>
        public string ModuleName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="moduleName"></param>
        public UnloadEventArgs(string moduleName)
        {
            ModuleName = moduleName;
        }
    }
}
#endif
