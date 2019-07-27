namespace McMaster.NETCore.Plugins.Manager
{
    /// <summary>
    /// Represents the method that will handle the ModuleLoaded or InactiveModuleLoaded event of ModuleManager
    /// </summary>
    /// <typeparam name="TModule"></typeparam>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ModuleEventHandler<TModule>(object sender, ModuleEventArgs<TModule> e);
}
