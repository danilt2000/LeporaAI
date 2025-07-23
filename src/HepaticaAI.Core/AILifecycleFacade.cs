using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Movement;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using HepaticaAI.Core.Interfaces.Vision;

namespace HepaticaAI.Core
{
    public class AILifecycleFacade(ILLMClient llmClient, IChatClient chatClient, IMovement movement,
        /*ISpeechRecognition speechRecognition,*/ /*VoiceMessageProcessorSelector voiceMessageProcessorSelector, */ChatMessageProcessorSelector chatMessageProcessorSelector
        , DiscordService discordService
        )
    {
        public async Task StartLife()
        {
            discordService.Initialization();
            //llmClient.Dispose();

            //llmClient.Initialize();//TODO REWRITE TO ANOTHER LLM or use server api

            //voiceMessageProcessorSelector.Start();//TODO REWRITE TO WORK WITH CHAT LISTENING 

            chatMessageProcessorSelector.Start();//TODO REWRITE TO WORK WITH CHAT LISTENING 

            movement.Initialize();

            chatClient.Connect();//Todo add possibility to disable chat listening

            //speechRecognition.Start();//TODO REWRITE TO WORK WITH CHAT LISTENING 
        }

        public void EndLife()
        {
            llmClient.Dispose();
        }
    }
}
