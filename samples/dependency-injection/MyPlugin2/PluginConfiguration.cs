// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MyPlugin2
{
    public class PluginConfiguration : IPluginFactory
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<IFruitConsumer, MyFruitConsumer>();
        }
    }
}
