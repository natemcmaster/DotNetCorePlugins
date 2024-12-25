// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;

namespace McMaster.NETCore.Plugins.Tests
{
    public class PrivateDependencyTests
    {
        [Fact]
        public void EachContextHasPrivateVersions()
        {
            var lib1context = PluginLoader.CreateFromAssemblyFile(TestResources.GetTestProjectAssembly("PrivateDepv1"));
            var lib2context = PluginLoader.CreateFromAssemblyFile(TestResources.GetTestProjectAssembly("PrivateDepv2"));
            var lib3context = PluginLoader.CreateFromAssemblyFile(TestResources.GetTestProjectAssembly("PrivateDepv3"));

            // Load newest first to prove we can load older assemblies later into the same process
            var lib3 = GetAssembly(lib3context);
            var lib2 = GetAssembly(lib2context);
            var lib1 = GetAssembly(lib1context);

            Assert.Equal(new Version("1.0.0.0"), lib1.GetName().Version);
            Assert.Equal(new Version("2.0.0.0"), lib2.GetName().Version);
            Assert.Equal(new Version("3.0.0.0"), lib3.GetName().Version);

            // types from each context have unique identities
            Assert.NotEqual(
                lib1.GetType("Mylib.Class1", throwOnError: true),
                lib2.GetType("Mylib.Class1", throwOnError: true));
            Assert.NotEqual(
              lib2.GetType("Mylib.Class1", throwOnError: true),
              lib3.GetType("Mylib.Class1", throwOnError: true));
        }

        private Assembly GetAssembly(PluginLoader loader)
            => loader.LoadAssembly(new AssemblyName("Mylib"));
    }
}
