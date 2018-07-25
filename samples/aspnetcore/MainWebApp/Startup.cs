using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Abstractions;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;

namespace MainWebApp
{
    public class Startup
    {
        private List<IWebPlugin> _plugins = new List<IWebPlugin>();

        public Startup()
        {
            foreach (var pluginFile in Directory.GetFiles(AppContext.BaseDirectory, "plugin.config", SearchOption.AllDirectories))
            {
                var loader = PluginLoader.CreateFromConfigFile(pluginFile,
                    // this ensures that the plugin resolves to the same version of DependencyInjection
                    // and ASP.NET Core that the current app uses
                    sharedTypes: new[]
                    {
                        typeof(IApplicationBuilder),
                        typeof(IWebPlugin),
                        typeof(IServiceCollection),
                    });
                foreach (var type in loader.LoadDefaultAssembly()
                    .GetTypes()
                    .Where(t => typeof(IWebPlugin).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    Console.WriteLine("Found plugin " + type.Name);
                    var plugin = (IWebPlugin)Activator.CreateInstance(type);
                    _plugins.Add(plugin);
                }
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(
                options => { options.ViewLocationExpanders.Add(new PluginViewLocationExpander()); });

            var mvcBuilder = services.AddMvc()
                .AddRazorOptions(o =>
                    {
                        foreach (var plugin in _plugins)
                        {
                            o.AdditionalCompilationReferences.Add(MetadataReference.CreateFromFile(plugin.GetType().Assembly.Location));
                        }
                    });

            foreach (var plugin in _plugins)
            {
                plugin.ConfigureServices(services);
                mvcBuilder.AddApplicationPart(plugin.GetType().Assembly);
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            foreach (var plugin in _plugins)
            {
                plugin.Configure(app);
            }

            app.UseMvcWithDefaultRoute();
        }
    }
}
