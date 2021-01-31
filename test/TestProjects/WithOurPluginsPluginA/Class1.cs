// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using WithOurPluginsPluginContract;

namespace WithOurPluginsPluginA
{
    public class Class1 : ISayHello
    {
        public string SayHello() => $"Hello from {nameof(WithOurPluginsPluginA)}";
    }
}
