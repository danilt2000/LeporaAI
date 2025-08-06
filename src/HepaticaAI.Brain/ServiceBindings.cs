using GenerativeAI;
using HepaticaAI.Brain.Jobs;
using HepaticaAI.Brain.Services;
using HepaticaAI.Brain.Services.Gemini;
using HepaticaAI.Brain.Services.Gpt4free;
using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using HepaticaAI.Core.Interfaces.Translations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.AspNetCore;

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

            //serviceCollection.AddSingleton<GeminiApiService>();
            serviceCollection.AddSingleton<ILLMClient, Gpt4freeLLMClient>();
            //serviceCollection.AddSingleton<ILLMClient, GeminiLLMClient>();
            //serviceCollection.AddSingleton<ILLMClient, KoboldCppLLMClient>();
            //serviceCollection.AddScoped<KoboldCppRunner>();
            serviceCollection.AddSingleton<IMemory, AIPromptsMemory>();
            serviceCollection.AddSingleton<ISystemPromptsUpdater, SystemPromptsUpdater>();
            serviceCollection.AddSingleton<ITranslation, DeplTranslation>();
            serviceCollection.AddSingleton<ISpeechRecognition, PythonWebSocketDiscordSpeechRecognition>();
            serviceCollection.AddSingleton<LlmToolCalling>();
            serviceCollection.AddSingleton<BrowserService>();

            serviceCollection.AddQuartz(q =>
            {
                var jobKeyStartStream = new JobKey("StartStreamJob");
                q.AddJob<StartStreamJob>(opts => opts.WithIdentity(jobKeyStartStream));

                q.AddTrigger(opts => opts
                    .ForJob(jobKeyStartStream)
                    .WithIdentity("StartStreamJob-trigger")
                    //.WithCronSchedule("0 0 16 * * ?", x => x // 16:00 UTC
                    //.InTimeZone(TimeZoneInfo.Utc))
                    .WithCronSchedule("0 59 3 * * ?", x => x
                        .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague")))
                );

                var jobKeyEndStream = new JobKey("EndStreamJob");
                q.AddJob<EndStreamJob>(opts => opts.WithIdentity(jobKeyEndStream));

                q.AddTrigger(opts => opts
                    .ForJob(jobKeyEndStream)
                    .WithIdentity("EndStreamJob-trigger")
                    .WithCronSchedule("0 0 22 * * ?", x => x // 22:00 UTC
                        .InTimeZone(TimeZoneInfo.Utc))
                );
            });

            serviceCollection.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return serviceCollection;
        }
    }

}