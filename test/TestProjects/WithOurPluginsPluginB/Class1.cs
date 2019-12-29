using System;
using WithOurPluginsPluginContract;

namespace WithOurPluginsPluginB
{
    public class Class1 : ISayHello
    {
        public string SayHello() => $"Hello from {nameof(WithOurPluginsPluginB)}";
    }
}
