using System;
using WithOurPluginsPluginContract;

namespace WithOurPluginsPluginA
{
    public class Class1 : ISayHello
    {
        public string SayHello() => $"Hello from {nameof(WithOurPluginsPluginA)}";
    }
}
