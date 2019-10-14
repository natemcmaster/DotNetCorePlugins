using Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mixer
{
    public class MixerPluginConfiguration : IPluginFactory
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<IMixerService, MixerService>();
            services.AddSingleton<IFruitService, StandardFruiteService>();
        }
    }
}
