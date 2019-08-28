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
