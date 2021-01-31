// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NETCOREAPP2_1

using Xunit;

namespace McMaster.NETCore.Plugins.Tests
{
    public class ShadowCopyTests
    {
        [Fact]
        public void DoesNotThrowWhenLoadingSameNativeDependecyMoreThanOnce()
        {
            var samplePath = TestResources.GetTestProjectAssembly("NativeDependency");

            using var loader = PluginLoader
                .CreateFromAssemblyFile(samplePath, config => config.EnableHotReload = true);

            var nativeDependencyLoadMethod = loader.LoadDefaultAssembly()
                ?.GetType("NativeDependency.NativeDependencyLoader")
                ?.GetMethod("Load");

            var exception = Record.Exception(() => nativeDependencyLoadMethod?.Invoke(null, null));

            Assert.Null(exception);
        }
    }
}

#endif
