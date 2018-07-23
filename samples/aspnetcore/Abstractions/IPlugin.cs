using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Plugin.Abstractions
{
    public interface IWebPlugin
    {
        void Configure(IApplicationBuilder appBuilder);
        void ConfigureServices(IServiceCollection services);
    }
}
