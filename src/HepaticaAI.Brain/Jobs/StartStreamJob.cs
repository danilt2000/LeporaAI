using HepaticaAI.Brain.Services;
using HepaticaAI.Core;
using HepaticaAI.Core.Interfaces.Vision;
using Quartz;

namespace HepaticaAI.Brain.Jobs
{
    public class StartStreamJob(ObsProcessor obsProcessor, IChatClient chatClient, BrowserService browserService) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var scheduledTimeUtc = DateTime.UtcNow.AddMinutes(1);

            var title = $"Test Stream {DateTime.UtcNow:HH:mm:ss}";
            var description = "Автоматически запланированный стрим (тест)";

            var livestreamId = await chatClient.ScheduleLivestreamAsync(
                scheduledTimeUtc,
                title,
                description
            );

            browserService.OpenStreamSettingsPage(livestreamId!);

            await Task.Delay(TimeSpan.FromMinutes(1));

            await obsProcessor.ConnectAsync("ws://localhost:4455", "123456");

            await obsProcessor.StartStreamAsync();
        }
    }
}
