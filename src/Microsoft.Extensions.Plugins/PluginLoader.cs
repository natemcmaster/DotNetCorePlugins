using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Plugins.ConfigModel;

namespace Microsoft.Extensions.Plugins
{
    public class PluginLoader
    {
        public static PluginLoader CreateFromConfigFile(string filePath)
        {
            return new PluginLoader(filePath);
        }

        private readonly string _mainAssembly;
        private AssemblyLoadContext _context;

        public Assembly LoadDefault()
        => _context.LoadFromAssemblyPath(_mainAssembly);

        public Assembly LoadAssembly(AssemblyName assemblyName)
            => _context.LoadFromAssemblyName(assemblyName);

        public Assembly LoadAssembly(string assemblyName)
            => LoadAssembly(new AssemblyName(assemblyName));

        internal PluginLoader(string configPath)
        {
            PluginConfig config;
            using (var reader = File.OpenText(configPath))
            {
                config = new PluginConfig(reader);
            }

            var baseDir = Path.GetDirectoryName(configPath);
            _mainAssembly = Path.Combine(baseDir, config.MainAssembly);
            _context = CreateLoadContext(baseDir, config);
        }

        internal PluginLoader(PluginConfig config, string baseDir)
        {
            _mainAssembly = Path.Combine(baseDir, config.MainAssembly);
            _context = CreateLoadContext(baseDir, config);
        }

        private static AssemblyLoadContext CreateLoadContext(string baseDir, PluginConfig config)
        {
            var depsJsonFile = Path.Combine(baseDir, Path.GetFileNameWithoutExtension(config.MainAssembly) + ".deps.json");

            var builder = new ManagedLoadContextBuilder();

            builder.PreferDefaultLoadContext(true);

            builder.TryAddDependencyContext(depsJsonFile);
            builder.SetBaseDirectory(baseDir);

            foreach (var ext in config.PrivateAssemblies)
            {
                builder.PreferLoadContextAssembly(ext);
            }

            var runtimeConfigFile = Path.Combine(baseDir, Path.GetFileNameWithoutExtension(config.MainAssembly) + ".runtimeconfig.json");

            builder.TryAddAdditionalProbingPathFromRuntimeConfig(runtimeConfigFile, includeDevConfig: true);

            return builder.Build();
        }
    }
}
