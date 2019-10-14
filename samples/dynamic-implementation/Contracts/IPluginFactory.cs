using Microsoft.Extensions.DependencyInjection;

namespace Contracts
{
    public interface IPluginFactory
    {
        void Configure(IServiceCollection services);
    }
}
