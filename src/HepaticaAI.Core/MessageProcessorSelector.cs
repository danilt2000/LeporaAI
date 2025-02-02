using System.Diagnostics;
using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using HepaticaAI.Core.Interfaces.Translations;
using HepaticaAI.Core.Interfaces.Movement;
namespace HepaticaAI.Core
{
    public class MessageProcessorSelector : IDisposable
    {
        private readonly IMemory _memory;
        private readonly ILLMClient _llmClient;
        private readonly ITranslation _translation;
        private readonly IMovement _movement;
        private readonly Timer _timer;
        private readonly Timer _idleTimer;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);
        private readonly TimeSpan _idleInterval = TimeSpan.FromSeconds(30);

        public MessageProcessorSelector(IMemory memory, ILLMClient llmClient, ITranslation translation, IMovement movement)
        {
            _memory = memory;
            _llmClient = llmClient;
            _translation = translation;
            _movement = movement;
            _timer = new Timer(Execute, null, TimeSpan.Zero, _interval);
            _idleTimer = new Timer(HandleIdleState, null, _idleInterval, Timeout.InfiniteTimeSpan);
        }

        private async void HandleIdleState(object? state)
        {
            //if (!_memory.HasMessagesToProcess() && _memory.IsNotCurrentlyProcessingMessage())
            //{
            //        _memory.StartProcessing();
            //        var messageToProcess = "*Никто ничего не пишет в чат нужно что то сказать*";

            //        var aiAnswer = await _llmClient.GenerateAsync("System", messageToProcess);

            //        Debug.WriteLine($"Ai answer:{aiAnswer}");

            //        var synthesizer = new SpeechSynthesizer();//TODO REWRITE IT TO ANOTHER TTS!!!!!!!!!!!!!

            //        synthesizer.SelectVoice("Microsoft Irina Desktop");

            //        synthesizer.Speak(aiAnswer);

            //        _memory.StopProcessing();
            //}

            //_idleTimer.Change(_idleInterval, Timeout.InfiniteTimeSpan);
        }

        public static bool IsMostlyRussian(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            int russianCount = Regex.Matches(text, "[а-яА-ЯёЁ]").Count;
            int englishCount = Regex.Matches(text, "[a-zA-Z]").Count;

            return russianCount >= englishCount;
        }

        private async void Execute(object? state)
        {
            if (_memory.HasMessagesToProcess() && _memory.IsNotCurrentlyProcessingMessage())
            {
                _memory.StartProcessing();

                var messageToProcess = _memory.GetMessageToProcess();

                var aiAnswer = await _llmClient.GenerateAsync(messageToProcess.Role, messageToProcess.Message);

                Debug.WriteLine($"Ai answer:{aiAnswer}");

                if (!IsMostlyRussian(aiAnswer))
                {
                    aiAnswer = await _translation.Translate(aiAnswer);
                }

                _movement.StartWinkAnimation();
                _movement.OpenMouth();
                var synthesizer = new SpeechSynthesizer();//TODO REWRITE IT TO ANOTHER TTS!!!!!!!!!!!!!

                synthesizer.SelectVoice("Microsoft Irina Desktop");

                synthesizer.Speak($"{messageToProcess.Role} {aiAnswer}");

                //_idleTimer.Change(_idleInterval, Timeout.InfiniteTimeSpan);//Todo think about deleting idle timer 

                _movement.CloseMouth();

                _memory.StopProcessing();
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}