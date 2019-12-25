using System.Runtime.Loader;

namespace WithOwnPluginsContract
{
    public interface IWithOwnPlugins
    {
        bool TryLoadPluginsInCustomContext(AssemblyLoadContext? callingContext);
    }
}
