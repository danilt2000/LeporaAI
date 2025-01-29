using Microsoft.Extensions.Hosting;

namespace HepaticaAI.Core
{
        internal class AIBackgroundService(AILifecycleFacade aiLifecycleFacade) : IHostedService
        {
                public Task StartAsync(CancellationToken cancellationToken)
                {
                        aiLifecycleFacade.StartLife();

                        return Task.CompletedTask;
                }

                public Task StopAsync(CancellationToken cancellationToken)
                {
                        aiLifecycleFacade.EndLife();

                        return Task.CompletedTask;
                }
        }
}
