using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Plugins.ConfigModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Extensions.Plugins
{
    public static class RuntimeConfigExtensions
    {
        private const string JsonExt = ".json";

        public static ManagedLoadContextBuilder TryAddAdditionalProbingPathFromRuntimeConfig(this ManagedLoadContextBuilder builder, string runtimeConfigPath, bool includeDevConfig)
        {
            try
            {
                var config = TryReadConfig(runtimeConfigPath);
                if (config == null)
                {
                    return builder;
                }

                RuntimeConfig devConfig = null;
                if (includeDevConfig)
                {
                    var configDevPath = runtimeConfigPath.Substring(0, runtimeConfigPath.Length - JsonExt.Length) + ".dev.json";
                    devConfig = TryReadConfig(configDevPath);
                }

                var tfm = config.runtimeOptions?.Tfm ?? devConfig?.runtimeOptions?.Tfm;

                if (config.runtimeOptions != null)
                {
                    AddProbingPaths(builder, config.runtimeOptions, tfm);
                }

                if (devConfig?.runtimeOptions != null)
                {
                    AddProbingPaths(builder, devConfig.runtimeOptions, tfm);
                }

                if (tfm != null)
                {
                    var dotnet = Process.GetCurrentProcess().MainModule.FileName;
                    if (string.Equals(Path.GetFileNameWithoutExtension(dotnet), "dotnet", StringComparison.OrdinalIgnoreCase))
                    {
                        var dotnetHome = Path.GetDirectoryName(dotnet);
                        builder.AddProbingPath(Path.Combine(dotnetHome, "store", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), tfm));
                    }
                }
            }
            catch { }
            return builder;
        }

        private static void AddProbingPaths(ManagedLoadContextBuilder builder, RuntimeOptions options, string tfm)
        {
            if (options.AdditionalProbingPaths == null)
            {
                return;
            }

            foreach (var item in options.AdditionalProbingPaths)
            {
                var path = item;
                if (path.Contains("|arch|"))
                {
                    path = path.Replace("|arch|", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant());
                }

                if (path.Contains("|tfm|"))
                {
                    if (tfm == null)
                    {
                        // We don't have enough information to parse this
                        continue;
                    }

                    path = path.Replace("|tfm|", tfm);
                }

                builder.AddProbingPath(path);
            }
        }

        private static RuntimeConfig TryReadConfig(string path)
        {
            try
            {
                using (var file = File.OpenText(path))
                using (var json = new JsonTextReader(file))
                {
                    var serializer = new JsonSerializer
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new CamelCaseNamingStrategy(),
                        },
                    };
                    return serializer.Deserialize<RuntimeConfig>(json);
                }
            }
            catch
            {
                return null;
            }
        }
    }

}
