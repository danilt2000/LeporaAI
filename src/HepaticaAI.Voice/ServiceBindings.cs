using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HepaticaAI.Voice
{
    public static class ServiceBindings
    {
        public static IServiceCollection AddVision(this IServiceCollection serviceCollection, IConfiguration configuration)
        {

            return serviceCollection;
        }
    }
}