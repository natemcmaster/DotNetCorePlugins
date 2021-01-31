// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Abstractions;

namespace Plugin2
{
    internal class WebPlugin2 : IWebPlugin, IPluginLink
    {
        public string GetHref() => "/plugin/v2";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPluginLink, WebPlugin2>();
        }

        public void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.Map("/plugin/v2", c =>
            {
                var autoMapperType = typeof(AutoMapper.IMapper).Assembly;
                c.Run(async (ctx) =>
                {
                    await ctx.Response.WriteAsync("This plugin uses " + autoMapperType.GetName().ToString());
                });
            });
        }
    }
}
