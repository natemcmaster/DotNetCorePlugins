// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace McMaster.NETCore.Plugins.LibraryModel
{
    /// <summary>
    /// Represents a managed, .NET assembly.
    /// </summary>
    [DebuggerDisplay("{Name} = {AdditionalProbingPath}")]
    public class ManagedLibrary
    {
        /// <summary>
        /// Name of the managed library
        /// </summary>
        public AssemblyName Name { get; private set; }

        /// <summary>
        /// Contains path to file within an additional probing path root. This is typically a combination
        /// of the NuGet package ID (lowercased), version, and path within the package.
        /// <para>
        /// For example, <c>microsoft.data.sqlite/1.0.0/lib/netstandard1.3/Microsoft.Data.Sqlite.dll</c>
        /// </para>
        /// </summary>
        public string AdditionalProbingPath { get; private set; }

        /// <summary>
        /// Create an instance of <see cref="ManagedLibrary" /> from a NuGet package.
        /// </summary>
        /// <param name="packageId">The name of the package.</param>
        /// <param name="packageVersion">The version of the package.</param>
        /// <param name="assetPath">The path within the NuGet package.</param>
        /// <returns></returns>
        public static ManagedLibrary CreateFromPackage(string packageId, string packageVersion, string assetPath)
        {
            return new ManagedLibrary
            {
                Name = new AssemblyName(Path.GetFileNameWithoutExtension(assetPath)),
                AdditionalProbingPath = Path.Combine(packageId.ToLowerInvariant(), packageVersion, assetPath),
            };
        }
    }
}
