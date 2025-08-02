using HepaticaAI.Core.Interfaces.Vision;
using HepaticaAI.Vision.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HepaticaAI.Vision
{
    public static class ServiceBindings
    {
        public static IServiceCollection AddVision(this IServiceCollection serviceCollection, IConfiguration configuration)
        {

            if (configuration["SttPlotformsToListen"] == "Youtube")
            {
                serviceCollection.AddSingleton<IChatClient, YoutubeChatClient>();
            }

            if (configuration["SttPlotformsToListen"] == "Twitch")
            {
                serviceCollection.AddSingleton<IChatClient, TwitchChatClient>();
            }

            return serviceCollection;
        }
    }
}