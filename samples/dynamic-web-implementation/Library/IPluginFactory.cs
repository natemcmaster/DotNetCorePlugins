using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library
{
    public interface IPluginFactory
    {
        void Configure(IServiceCollection services);
    }
}
