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
            // Overraide implementation
            services.Replace(ServiceDescriptor.Singleton<IFruitService, FruitOverrideService>());
        }
    }
}
