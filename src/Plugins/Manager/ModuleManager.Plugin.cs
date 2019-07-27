#if FEATURE_UNLOAD
namespace McMaster.NETCore.Plugins.Manager
{
    public sealed partial class ModuleManager<TType> where TType : class
    {
        private class Plugin
        {
            public string FullPath { get; }
            public int LoaderHashCode { get; }

            public Plugin(string fullPath, int loaderHashCode)
            {
                FullPath = fullPath;
                LoaderHashCode = loaderHashCode;
            }
        }
    }
}
#endif
