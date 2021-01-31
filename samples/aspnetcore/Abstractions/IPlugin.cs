// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Plugin.Abstractions
{
    public interface IWebPlugin
    {
        void Configure(IApplicationBuilder appBuilder);
        void ConfigureServices(IServiceCollection services);
    }
}
