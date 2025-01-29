using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HepaticaAI.Brain
{
        public static class ServiceBindings
        {
                public static IServiceCollection AddBrain(this IServiceCollection serviceCollection, IConfiguration configuration)
                {
                        //serviceCollection.AddSingleton<IObsidianProcessor, ObsidianProcessor>();

                        //serviceCollection.AddLogging(config =>
                        //{
                        //        config.AddProvider(new ApiLoggerProvider(new TelegramBotMessengerSender()));

                        //        config.SetMinimumLevel(LogLevel.Error);
                        //});

                        return serviceCollection;
                }
        }
}