using System.Diagnostics;
using System.IO;

namespace Microsoft.Extensions.Plugins
{
    [DebuggerDisplay("{Name} = {AdditionalProbingPath}")]
    public class NativeLibrary
    {
        /// <summary>
        /// Name of the native library
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Contains path to file within a deployed application
        /// runtimes/linux-x64/native/libsqlite.so
        /// </summary>
        public string AppLocalPath { get; private set; }

        /// <summary>
        /// Contains path to file within an additional probing path root.
        /// sqlite/3.13.3/runtimes/linux-x64/native/libsqlite.so
        /// </summary>
        public string AdditionalProbingPath { get; private set; }

        public static NativeLibrary Create(string libraryName, string version, string assetPath)
        {
            return new NativeLibrary
            {
                Name = Path.GetFileNameWithoutExtension(assetPath),
                AppLocalPath = assetPath,
                AdditionalProbingPath = Path.Combine(libraryName.ToLowerInvariant(), version, assetPath),
            };
        }
    }
}
