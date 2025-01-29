using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HepaticaAI.Core
{
        public static class ServiceBindings
        {
                public static IServiceCollection AddCore(this IServiceCollection serviceCollection, IConfiguration configuration)
                {
                        serviceCollection.AddSingleton<AILifecycleFacade>();

                        serviceCollection.AddSingleton<IHostedService, AIBackgroundService>();

                        //serviceCollection.AddLogging(config =>
                        //{
                        //        config.AddProvider(new ApiLoggerProvider(new TelegramBotMessengerSender()));

                        //        config.SetMinimumLevel(LogLevel.Error);
                        //});

                        return serviceCollection;
                }
        }
}
