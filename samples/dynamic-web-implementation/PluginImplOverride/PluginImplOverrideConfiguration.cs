using Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PluginImplOverride.Classes;
using PluginLib.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginImplOverride
{
    public class PluginImplOverrideConfiguration : IPluginFactory
    {
        public void Configure(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(x => x.ServiceType.Name == "IFruitService");
            var t = services.Where(x => x.ServiceType.Name == "IFruitService").ToList();
            services.Remove(descriptor);
            services.AddSingleton<IFruitService, FruitOverrideService>();
            //services.Replace(ServiceDescriptor.Singleton<IFruitService, FruitOverrideService>());
        }
    }
}
