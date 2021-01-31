// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceImplementation
{
    public class OverridePluginConfiguration : IPluginFactory
    {
        public void Configure(IServiceCollection services)
        {
            //this service override the standard one. unload this plugin or comment this to use the basic service
            services.AddSingleton<IFruitService, OverrideFruiteService>();
        }
    }
}
