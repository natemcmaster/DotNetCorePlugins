using Contracts;
using Microsoft.Extensions.DependencyInjection;
using ServiceImplementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceImplementation
{
    public class OverridePluginConfiguration : IPluginFactory
    {
        public void Configure(IServiceCollection services)
        {
           // services.AddSingleton<IFruitService, OverrideFruiteService>();
        }
    }
}
