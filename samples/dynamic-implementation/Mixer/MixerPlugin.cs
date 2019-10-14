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
