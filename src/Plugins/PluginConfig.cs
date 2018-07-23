using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace McMaster.Extensions.Plugins
{
    /// <summary>
    /// Represents the configuration for a .NET Core plugin.
    /// </summary>
    public class PluginConfig
    {
        /// <summary>
        /// Initialize an instance of <see cref="PluginConfig" />.
        /// </summary>
        /// <param name="mainAssembly">The name of the main assembly.</param>
        /// <param name="privateAssembly">A list of assemblies to treat as private, if possible.</param>
        protected PluginConfig(AssemblyName mainAssembly, IReadOnlyCollection<AssemblyName> privateAssembly)
        {
            MainAssembly = mainAssembly ?? throw new ArgumentNullException(nameof(mainAssembly));
            PrivateAssemblies = privateAssembly ?? throw new ArgumentNullException(nameof(privateAssembly));
        }


        /// <summary>
        /// Create an instance of <see cref="PluginConfig" /> from a file.
        /// </summary>
        /// <param name="filePath">The path the config file.</param>
        /// <returns></returns>
        public static PluginConfig CreateFromFile(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                return PluginConfig.CreateFromReader(reader);
            }
        }

        /// <summary>
        /// Create an instance of <see cref="PluginConfig" /> from a file.
        /// </summary>
        /// <param name="reader">The reader containing the config file.</param>
        /// <returns></returns>

        public static PluginConfig CreateFromReader(TextReader reader)
        {
            var privateDeps = new HashSet<AssemblyName>();
            var doc = XDocument.Load(reader, LoadOptions.SetLineInfo);

            if (doc.Root.Name != "PluginConfig")
            {
                throw new InvalidDataException("Root element should be 'PluginConfig'");
            }

            var mainAssemblyAttr = doc.Root.Attribute("MainAssembly");
            if (mainAssemblyAttr == null || string.IsNullOrEmpty(mainAssemblyAttr.Value))
            {
                IXmlLineInfo line = doc.Root;
                throw new InvalidDataException($"Missing required attribute 'MainAssembly' for PluginConfig on line {line.LineNumber}");
            }

            var mainAssembly = new AssemblyName(mainAssemblyAttr.Value);

            foreach (var dep in doc.Root.Descendants("PrivateDependency"))
            {
                var identity = dep.Attribute("Identity");
                if (identity == null || string.IsNullOrEmpty(identity.Value))
                {
                    IXmlLineInfo line = dep;
                    throw new InvalidDataException($"Missing required attribute 'Identity' for PrivateDependency on line {line.LineNumber}");
                }

                privateDeps.Add(new AssemblyName(identity.Value));
            }

            return new PluginConfig(mainAssembly, privateDeps);
        }

        /// <summary>
        /// A list of assemblies which should be treated as private.
        /// </summary>
        public IReadOnlyCollection<AssemblyName> PrivateAssemblies { get; protected set; }

        /// <summary>
        /// The name of the main assembly.
        /// </summary>
        public AssemblyName MainAssembly { get; protected set; }
    }
}
