using HepaticaAI.Core;
using Quartz;

namespace HepaticaAI.Brain.Jobs
{
    public class EndStreamJob(ObsProcessor obsProcessor) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await obsProcessor.StopStreamAsync();

            await obsProcessor.DisposeAsync();
        }
    }
}
