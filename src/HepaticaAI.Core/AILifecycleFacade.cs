using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Movement;
using HepaticaAI.Core.Interfaces.Vision;

namespace HepaticaAI.Core
{
        public class AILifecycleFacade(ILLMClient llmClient, IChatClient chatClient, IMovement movement)
        {
                public void StartLife()
                {
                        llmClient.Dispose();

                        llmClient.Initialize();

                        movement.Initialize();

                        chatClient.Connect();
                }

                public void EndLife()
                {
                        llmClient.Dispose();
                }

        }
}
