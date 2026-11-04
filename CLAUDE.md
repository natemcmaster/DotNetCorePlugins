# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DotNetCorePlugins is a .NET library for dynamically loading .NET assemblies as plugins with fine-grained control over assembly isolation and type sharing. This is the Hosihikari fork (see [FORK.md](FORK.md)); it targets .NET 10.

## Build Commands

```bash
# Full build (format, build, pack, test)
pwsh ./build.ps1

# CI build (Release configuration)
pwsh ./build.ps1 -ci

# Individual commands
dotnet format                    # Code formatting
dotnet build                     # Build all projects
dotnet test test/Hosihikari.Tests -c Release   # RID probe tests
dotnet pack -o artifacts/        # Create NuGet packages
```

## CI

Include `[ci skip]` in a commit message to skip CI builds.

## Architecture

### Core Components

- **McMaster.NETCore.Plugins** (`src/Plugins/`): Main library providing `PluginLoader` API
  - `PluginLoader.cs`: Primary entry point for loading plugins from assembly files
  - `PluginConfig.cs`: Configuration options (shared types, unloadable, hot reload)
  - `Loader/ManagedLoadContext.cs`: Custom AssemblyLoadContext implementation
  - `Loader/AssemblyLoadContextBuilder.cs`: Builds the load context with dependencies

- **McMaster.NETCore.Plugins.Mvc** (`src/Plugins.Mvc/`): ASP.NET Core MVC/Razor integration

### Key Concepts

**Type Sharing**: The central design concept. By default, each plugin has isolated assemblies. Shared types allow passing objects across the plugin/host boundary by unifying specified types to use the host's version.

**AssemblyLoadContext**: .NET's built-in assembly isolation mechanism. The library creates custom contexts that read `.deps.json` and `.runtimeconfig.json` to resolve dependencies.

### Test Structure

- `test/Plugins.Tests/`: xUnit tests
- `test/TestProjects/`: 30+ test assemblies for various plugin scenarios
- `test/Hosihikari.Tests/`: the RID-specific native probe

Upstream `samples/` is not carried in this fork.

## Code Conventions

Enforced via `.editorconfig` with build errors:

- **Private fields**: `_camelCase`
- **Private static fields**: `s_camelCase`
- **Constants**: `PascalCase`
- **Braces**: Always required
- **Using directives**: Outside namespace, System first

Required file header on all C# files:
```csharp
// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
```

## Build Configuration

- Strong name signing enabled (`src/StrongName.snk`)
- Warnings as errors enabled
- Nullable reference types enabled
- C# 12 with implicit usings
- Build outputs go to `.build/bin/` and `.build/obj/`
