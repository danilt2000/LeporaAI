using HepaticaAI.Brain.Services;
using HepaticaAI.Core.Interfaces.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HepaticaAI.Brain
{
        public static class ServiceBindings
        {
                public static IServiceCollection AddBrain(this IServiceCollection serviceCollection, IConfiguration configuration)
                {
                        serviceCollection.AddSingleton<ILLMClient, KoboldCppLLMClient>();
                        serviceCollection.AddScoped<KoboldCppRunner>();

                        return serviceCollection;
                }
        }
}