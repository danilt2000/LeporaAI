using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Movement;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using HepaticaAI.Core.Interfaces.Vision;

namespace HepaticaAI.Core
{
    public class AILifecycleFacade(ILLMClient llmClient, IChatClient chatClient, IMovement movement, ISpeechRecognition speechRecognition, MessageProcessorSelector messageProcessorSelector)
    {
        public async Task StartLife()
        {
            //llmClient.Dispose();

            //llmClient.Initialize();

            messageProcessorSelector.Start();

            movement.Initialize();

            chatClient.Connect();//Todo add possibility to disable chat listening

            speechRecognition.Start();
        }

        public void EndLife()
        {
            llmClient.Dispose();
        }
    }
}
