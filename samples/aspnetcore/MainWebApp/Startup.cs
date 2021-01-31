// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Abstractions;

namespace MainWebApp
{
    public class Startup
    {
        private readonly List<IWebPlugin> _plugins = new();

        public Startup()
        {
            foreach (var pluginDir in Directory.GetDirectories(Path.Combine(AppContext.BaseDirectory, "plugins")))
            {
                var dirName = Path.GetFileName(pluginDir);
                var pluginFile = Path.Combine(pluginDir, dirName + ".dll");
                var loader = PluginLoader.CreateFromAssemblyFile(pluginFile,
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
            services.AddMvc();

            foreach (var plugin in _plugins)
            {
                plugin.ConfigureServices(services);
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
