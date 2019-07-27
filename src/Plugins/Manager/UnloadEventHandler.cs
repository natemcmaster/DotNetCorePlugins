#if FEATURE_UNLOAD
namespace McMaster.NETCore.Plugins.Manager
{
    /// <summary>
    /// Represents the method that will handle the ModuleUnloaded or InactiveModuleUnloaded event of ModuleManager
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void UnloadEventHandler(object sender, UnloadEventArgs e);
}
#endif
