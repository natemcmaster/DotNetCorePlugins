using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Library;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC Services
            var builder = services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //*****************************//
            // Call First impelementazion //

            //FirstScenario(services);

            //*****************************//
            // Call second impelementazion //

            //SecondScenario(builder, services);

            //*****************************//
            // Call Third impelementazion //

            ThirdScenario(services);

            // Register controller
            var a = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("PluginLib")).FirstOrDefault();
            builder.AddApplicationPart(a).AddControllersAsServices();
            //var prov = services.BuildServiceProvider();
        }

        /// <summary>
        /// Simple scenario that load minimun types
        /// </summary>
        /// <param name="services"></param>
        private void FirstScenario(IServiceCollection services)
        {
            var sharedTyes = new List<Type> { typeof(IServiceProvider), typeof(IServiceCollection), typeof(IPluginFactory) };
            //Load plugin library
            var assLib = Path.Combine(AppContext.BaseDirectory, @"PluginLib\PluginLib\PluginLib.dll");
            var libLoader = PluginLoader.CreateFromAssemblyFile(
                       assLib,
                       sharedTyes.ToArray(),
                       conf => {
                           conf.PreferSharedTypes = true;
                       });
            var assLoadedLib = libLoader.LoadDefaultAssembly();

            //Load plugin implementation
            var assImpl = Path.Combine(AppContext.BaseDirectory, @"PluginImpl\PluginImpl\PluginImpl.dll");
            var implLoader = PluginLoader.CreateFromAssemblyFile(
                       assImpl,
                       sharedTyes.ToArray(),
                       conf => {
                           conf.PreferSharedTypes = true;
                       });
            var implLoadedLib = implLoader.LoadDefaultAssembly();

            // Invoke configuration
            var configType = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("PluginImpl")).First().GetTypes()
                .Where(t => typeof(IPluginFactory).IsAssignableFrom(t) && !t.IsAbstract).FirstOrDefault();
            var plugin = Activator.CreateInstance(configType) as IPluginFactory;
            plugin.Configure(services);
        }

        /// <summary>
        /// Scenario with Plugin MVC registration
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        private void SecondScenario(IMvcBuilder builder, IServiceCollection services)
        {
            
            //Load plugin library
            var assLib = Path.Combine(AppContext.BaseDirectory, @"PluginLib\PluginLib\PluginLib.dll");
            builder = builder.AddPluginFromAssemblyFile(assLib);

            //Load plugin implementation
            var assImpl = Path.Combine(AppContext.BaseDirectory, @"PluginImpl\PluginImpl\PluginImpl.dll");
            builder = builder.AddPluginFromAssemblyFile(assImpl);

            // Invoke configuration
            var configType = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("PluginImpl")).First().GetTypes()
                .Where(t => typeof(IPluginFactory).IsAssignableFrom(t) && !t.IsAbstract).FirstOrDefault();
            var plugin = Activator.CreateInstance(configType) as IPluginFactory;
            plugin.Configure(services);
        }

        private void ThirdScenario(IServiceCollection services)
        {
            var sharedTyes = new List<Type> { typeof(IServiceProvider), typeof(IServiceCollection), typeof(IPluginFactory) };
            ////Load plugin library
            //var assLib = Path.Combine(AppContext.BaseDirectory, @"PluginLib\PluginLib\PluginLib.dll");
            //var libLoader = PluginLoader.CreateFromAssemblyFile(
            //           assLib,
            //           sharedTyes.ToArray(),
            //           conf => {
            //               conf.PreferSharedTypes = true;
            //           });
            //var assLoadedLib = libLoader.LoadDefaultAssembly();

            //Load plugin implementation
            var assImpl = Path.Combine(AppContext.BaseDirectory, @"PluginImpl\PluginImpl\PluginImpl.dll");
            var implLoader = PluginLoader.CreateFromAssemblyFile(
                       assImpl,
                       sharedTyes.ToArray(),
                       conf => {
                           conf.PreferSharedTypes = true;
                       });
            var implLoadedLib = implLoader.LoadDefaultAssembly();

            // Invoke configuration
            var configType = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("PluginImpl")).First().GetTypes()
                .Where(t => typeof(IPluginFactory).IsAssignableFrom(t) && !t.IsAbstract).FirstOrDefault();
            var plugin = Activator.CreateInstance(configType) as IPluginFactory;
            plugin.Configure(services);
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private static List<Assembly> GetAllAssembly()
        {
            var allAssembly = new List<Assembly>();
            allAssembly.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            allAssembly.Add(Assembly.GetExecutingAssembly());
            allAssembly.Add(Assembly.GetEntryAssembly());
            return allAssembly;
        }
    }
}
