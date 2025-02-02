using HepaticaAI.Core.Interfaces.Movement;
using HepaticaAI.Movement.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HepaticaAI.Movement
{
        public static class ServiceBindings
        {
                public static IServiceCollection AddMovement(this IServiceCollection serviceCollection, IConfiguration configuration)
                {
                        serviceCollection.AddSingleton<IMovement, VtubeStudioMovement>();

                        //serviceCollection.AddLogging(config =>
                        //{
                        //        config.AddProvider(new ApiLoggerProvider(new TelegramBotMessengerSender()));

                        //        config.SetMinimumLevel(LogLevel.Error);
                        //});

                        return serviceCollection;
                }
        }
}