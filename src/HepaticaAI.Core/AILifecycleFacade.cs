using HepaticaAI.Core.Interfaces.AI;

namespace HepaticaAI.Core
{
        internal class AILifecycleFacade(ILLMClient llmClient)
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
