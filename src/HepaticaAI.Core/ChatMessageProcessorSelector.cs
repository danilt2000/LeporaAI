using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.Movement;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using HepaticaAI.Core.Interfaces.Translations;
using HepaticaAI.Core.Interfaces.Voice;
using HepaticaAI.Core.Models;
using HepaticaAI.Core.Models.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace HepaticaAI.Core
{
    public class ChatMessageProcessorSelector
    {
        private readonly IMemory _memory;
        private readonly ILLMClient _llmClient;
        private readonly ITranslation _translation;
        private readonly IMovement _movement;
        private readonly IVoiceSynthesis _voice;
        private readonly ISpeechRecognition _speechRecognition;
        private readonly Timer _timer;
        private readonly SemaphoreSlim _semaphore = new(5);
        private readonly ConcurrentQueue<(Guid, MessageEntry)> _chatMessageQueue = new();//TODO ADD CLEAR DELETING POSSIBILITIES FOR MESSAGE QUEUES
        private readonly List<MessageEntry> _voiceChatMessageQueue = new();//TODO ADD CLEAR DELETING POSSIBILITIES FOR MESSAGE QUEUES
        private readonly ConcurrentDictionary<Guid, MessageForVoiceToProcess> _generatedChatResponses = new();//TODO ADD CLEAR DELETING POSSIBILITIES FOR MESSAGE QUEUES
        public string CurrentSpeakAudioPath = string.Empty;//TODO ADD CLEAR DELETING POSSIBILITIES FOR MESSAGE QUEUES
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);
        public bool IsNotPlayingIntermediateOrFinalSpeech { get; set; } = true;

        public DateTime LastIsNotPlayingIntermediateOrFinalSpeechChange { get; set; } = DateTime.MinValue;

        public void SetFalseIsNotPlayingIntermediateSpeech()
        {
            IsNotPlayingIntermediateOrFinalSpeech = false;

            LastIsNotPlayingIntermediateOrFinalSpeechChange = DateTime.UtcNow;
        }

        public ChatMessageProcessorSelector(IMemory memory, ILLMClient llmClient, ITranslation translation, IMovement movement, IVoiceSynthesis voice/*, ISpeechRecognition speechRecognition*/)
        {
            _memory = memory;
            _llmClient = llmClient;
            _translation = translation;
            _movement = movement;
            _voice = voice;
            _timer = new Timer(Execute, null, TimeSpan.Zero, _interval);
            //_speechRecognition = speechRecognition;
            //_timer = new Timer(EnqueueMessages, null, TimeSpan.Zero, _interval);
            //_timer = new Timer(EnqueueVoiceMessages, null, TimeSpan.Zero, _interval);
            //_timer = new Timer(UpdateIsNotPlayingIntermediateOrFinalSpeechState, null, TimeSpan.Zero, _interval);
            //_timer = new Timer(TimerCallback, null, TimeSpan.Zero, _interval);

            //Task.Run(ProcessMessagesAsync);
            //Task.Run(SpeakNextMessage);
        }

        public void Start()
        {

        }

        private async void Execute(object? state)
        {
            if (_memory.HasMessagesToProcess() && _memory.IsNotCurrentlyProcessingMessage())
            {
                _memory.StartProcessing();

                var chatMessagesToProcess = _memory.GetChatMessagesToProcess();

                var aiAnswer = await _llmClient.GenerateAsync(chatMessagesToProcess);

                aiAnswer = aiAnswer
                    .Replace("*", string.Empty);

                _memory.AddEntities(chatMessagesToProcess);

                Debug.WriteLine($"Ai answer:{aiAnswer}");

                _movement.StartWinkAnimation();
                _movement.OpenMouth();
                var synthesizer = new SpeechSynthesizer();//TODO REWRITE IT TO ANOTHER TTS!!!!!!!!!!!!!

                synthesizer.SelectVoice("Microsoft Irina Desktop");

                synthesizer.Speak($"{aiAnswer}");
                //synthesizer.Speak($"{messageToProcess.Role} {aiAnswer}");

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
