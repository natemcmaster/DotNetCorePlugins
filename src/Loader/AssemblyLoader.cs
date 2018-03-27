using System.IO;
using System.Reflection;

namespace System.Runtime.Loader
{
    public class AssemblyLoader
    {
        private readonly DependencyLoadContext _context;

        public static AssemblyLoader CreateFromFile(string filePath)
        {
            var builder = new AssemblyLoaderBuilder();
            var name = Path.GetFileNameWithoutExtension(filePath);
            builder.AddManagedLibrary(name, filePath);
            builder.AddSearchPath(Path.GetDirectoryName(filePath));
            builder.SetDefaultAssemblyName(name);
            return builder.Build();
        }

        internal AssemblyLoader(DependencyLoadContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Assembly LoadDefault()
            => _context.LoadDefaultAssembly();

        public Assembly LoadAssembly(AssemblyName assemblyName)
            => _context.LoadFromAssemblyName(assemblyName);

        public Assembly LoadAssembly(string assemblyName)
            => LoadAssembly(new AssemblyName(assemblyName));
    }
}
