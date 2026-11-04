// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using McMaster.NETCore.Plugins.Loader;
using Xunit;

namespace Hosihikari.McMaster.NETCore.Plugins.Tests
{
    public class NativeRidPathTests
    {
        [Fact]
        public void PrefersRuntimeSpecificNativeDirectory()
        {
            var basePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                var nativeDirectory = Path.Combine(basePath, "runtimes", RuntimeInformation.RuntimeIdentifier, "native");
                Directory.CreateDirectory(nativeDirectory);
                var fileName = OperatingSystem.IsWindows() ? "probe.dll"
                    : OperatingSystem.IsMacOS() ? "libprobe.dylib"
                    : "libprobe.so";
                var expected = Path.Combine(nativeDirectory, fileName);
                File.WriteAllText(expected, "");

                Assert.Equal(expected, ManagedLoadContext.GetRuntimeSpecificNativeLibraryPath(basePath, "probe"));
                Assert.Null(ManagedLoadContext.GetRuntimeSpecificNativeLibraryPath(basePath, "missing"));
            }
            finally
            {
                if (Directory.Exists(basePath))
                {
                    Directory.Delete(basePath, recursive: true);
                }
            }
        }
    }
}
