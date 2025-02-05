using HepaticaAI.Core.Interfaces.Voice;
using HepaticaAI.Voice.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HepaticaAI.Voice
{
    public static class ServiceBindings
    {
        public static IServiceCollection AddVoice(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IVoiceSynthesis, EdgeTtsVoiceSynthesis>();

            serviceCollection.AddSingleton<IAudioPlayer, NAudioPlayer>();

            return serviceCollection;
        }
    }
}