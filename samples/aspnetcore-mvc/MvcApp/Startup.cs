using System;
using System.IO;
using System.Reflection;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace MvcWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            foreach (var dir in Directory.GetDirectories(Path.Combine(AppContext.BaseDirectory, "plugins")))
            {
                var pluginName = Path.GetFileName(dir);
                var plugin = PluginLoader.CreateFromAssemblyFile(Path.Combine(dir, pluginName + ".dll"));
                var pluginAssembly = plugin.LoadDefaultAssembly();
                Console.WriteLine($"Loading application parts from plugin {pluginName}");

                // This loads MVC application parts from plugin assemblies
                var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
                foreach (var part in partFactory.GetApplicationParts(pluginAssembly))
                {
                    Console.WriteLine($"* {part.Name}");
                    mvcBuilder.PartManager.ApplicationParts.Add(part);
                }

                // This piece finds and loads related parts, such as MvcAppPlugin1.Views.dll.
                var relatedAssembliesAttrs = pluginAssembly.GetCustomAttributes<RelatedAssemblyAttribute>();
                foreach (var attr in relatedAssembliesAttrs)
                {
                    var assembly = plugin.LoadAssembly(attr.AssemblyFileName);
                    partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                    foreach (var part in partFactory.GetApplicationParts(assembly))
                    {
                        Console.WriteLine($"  * {part.Name}");
                        mvcBuilder.PartManager.ApplicationParts.Add(part);
                    }
                }
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();
        }
    }
}
