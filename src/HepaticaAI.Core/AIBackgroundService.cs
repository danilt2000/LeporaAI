using Microsoft.Extensions.Hosting;

namespace HepaticaAI.Core
{//Todo if not used delete this class 
        internal class AIBackgroundService(AILifecycleFacade aiLifecycleFacade) : IHostedService
        {
                public async Task StartAsync(CancellationToken cancellationToken)
                {
                        await aiLifecycleFacade.StartLife();
                }

                public Task StopAsync(CancellationToken cancellationToken)
                {
                        aiLifecycleFacade.EndLife();

                        return Task.CompletedTask;
                }
        }
}
