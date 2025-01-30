using HepaticaAI.Core.Interfaces.AI;

namespace HepaticaAI.Core
{
        public class AILifecycleFacade(ILLMClient llmClient) 
        {
                public void StartLife()
                {
                        llmClient.Initialize();
                }

                public void EndLife()
                {
                }

        }
}
