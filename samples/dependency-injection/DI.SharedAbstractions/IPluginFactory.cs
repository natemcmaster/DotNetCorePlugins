// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection
{
    public interface IPluginFactory
    {
        void Configure(IServiceCollection services);
    }
}
