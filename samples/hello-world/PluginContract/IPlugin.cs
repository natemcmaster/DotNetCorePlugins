// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace HelloWorld
{
    /// <summary>
    /// This interface is an example of one way to define the interactions between the host and plugins.
    /// There is nothing special about the name "IPlugin"; it's used here to illustrate a concept.
    /// Look at https://github.com/natemcmaster/DotNetCorePlugins/tree/master/samples for additional examples
    /// of ways you could define the interaction between host and plugins.
    /// </summary>
    public interface IPlugin
    {
        string GetName();
    }
}
