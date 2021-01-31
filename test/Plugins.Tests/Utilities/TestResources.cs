// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;

namespace McMaster.NETCore.Plugins.Tests
{
    public class TestResources
    {
        public static string GetTestProjectAssembly(string name)
        {
            return typeof(TestResources)
                .Assembly
                .GetCustomAttributes<TestProjectReferenceAttribute>()
                .First(a => a.Name == name)
                .Path;
        }
    }
}
