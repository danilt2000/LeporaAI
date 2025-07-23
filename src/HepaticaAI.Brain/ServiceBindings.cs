using GenerativeAI;
using HepaticaAI.Brain.Services;
using HepaticaAI.Brain.Services.Gemini;
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
            serviceCollection.AddSingleton<IGenerativeModel>(_ =>
            {
                var apiKey = configuration["Gemini:ApiKey"];
                return new GenerativeModel(apiKey!, "gemini-2.0-flash");
            });

            serviceCollection.AddSingleton<GeminiApiService>();
            serviceCollection.AddSingleton<ILLMClient, GeminiLLMClient>();
            //serviceCollection.AddSingleton<ILLMClient, KoboldCppLLMClient>();
            //serviceCollection.AddScoped<KoboldCppRunner>();
            serviceCollection.AddSingleton<IMemory, AIPromptsMemory>();
            serviceCollection.AddSingleton<ISystemPromptsUpdater, SystemPromptsUpdater>();
            serviceCollection.AddSingleton<ITranslation, DeplTranslation>();
            serviceCollection.AddSingleton<ITranslation, DeplTranslation>();
            serviceCollection.AddSingleton<ISpeechRecognition, PythonWebSocketDiscordSpeechRecognition>();//TODO UNCOMMIT AFTER STARTING TO WORK WITH DISCORD 

            return serviceCollection;
        }
    }
}