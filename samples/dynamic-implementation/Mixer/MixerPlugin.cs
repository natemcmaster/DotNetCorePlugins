// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contracts;
using Microsoft.Extensions.DependencyInjection;

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
