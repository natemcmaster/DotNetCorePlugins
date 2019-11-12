using Library;
using Microsoft.Extensions.DependencyInjection;
using PluginImpl.Classes;
using PluginLib.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PluginImpl
{
    public class PluginImplConfiguration : IPluginFactory
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<IFruitService, FruitService>();
        }
    }
}
