# Hosihikari fork of DotNetCorePlugins

This repository is a fork of
[natemcmaster/DotNetCorePlugins](https://github.com/natemcmaster/DotNetCorePlugins) at commit
`bcf9c8bd6d50097cfea1c3eeba132b571410fabe`, used by the Hosihikari framework.

## Changes

- `ManagedLoadContext.LoadUnmanagedDll` probes
  `runtimes/<RuntimeInformation.RuntimeIdentifier>/native` before `.deps.json`-based native
  resolution, so a plugin package can ship RID-specific native assets without a deps file.
- Package ID is `Hosihikari.McMaster.NETCore.Plugins`.
- `test/Hosihikari.Tests` covers the RID-specific path probe.
- Upstream `samples/` removed; `DotNetCorePlugins.sln` contains only the library and the
  Hosihikari test project.

## Publishing

`.github/workflows/publish.yml` publishes `Hosihikari.McMaster.NETCore.Plugins` to nuget.org
through Trusted Publishing when a `v*` tag is pushed. The tag must equal `<PackageVersion>` in
`src/Plugins/McMaster.NETCore.Plugins.csproj`; the workflow checks this before packing and pushes
both the package and its symbols.

## Licence

Apache-2.0, the same as upstream. The original copyright and licence notices are retained.
Modified files: `src/Plugins/Loader/ManagedLoadContext.cs`,
`src/Plugins/McMaster.NETCore.Plugins.csproj`, `src/Plugins/Properties/AssemblyInfo.cs`,
`README.md`, `DotNetCorePlugins.sln`, `.github/workflows/ci.yml`; added `FORK.md` and
`test/Hosihikari.Tests`; removed `samples/`. See [LICENSE.txt](LICENSE.txt).
