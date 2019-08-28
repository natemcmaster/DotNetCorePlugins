using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection
{
    public interface IPluginFactory
    {
        void Configure(IServiceCollection services);
    }
}
