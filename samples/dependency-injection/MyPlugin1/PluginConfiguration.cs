using DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MyPlugin1
{
    public class PluginConfiguration : IPluginFactory
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<IFruitProducer, MyFruitProducer>();
        }
    }
}
