using HepaticaAI.Brain.Services;
using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using HepaticaAI.Core.Interfaces.Translations;
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
                        serviceCollection.AddSingleton<IMemory, AIPromptsMemory>();
                        serviceCollection.AddSingleton<ITranslation, DeplTranslation>();
                        serviceCollection.AddSingleton<ISpeechRecognition, DiscordSpeechRecognition>();

                        return serviceCollection;
                }
        }
}