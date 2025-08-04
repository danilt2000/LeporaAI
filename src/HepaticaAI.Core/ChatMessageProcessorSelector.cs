using Discord;
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
using System.Text.Json;

namespace HepaticaAI.Core
{
    public class ChatMessageProcessorSelector
    {
        private readonly SocketViewerWebSocketBridge _socketViewerWebSocketBridge;
        private readonly DiscordService _discordService;
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
        private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(100);
        public bool IsNotPlayingIntermediateOrFinalSpeech { get; set; } = true;

        public DateTime LastIsNotPlayingIntermediateOrFinalSpeechChange { get; set; } = DateTime.MinValue;

        public void SetFalseIsNotPlayingIntermediateSpeech()
        {
            IsNotPlayingIntermediateOrFinalSpeech = false;

            LastIsNotPlayingIntermediateOrFinalSpeechChange = DateTime.UtcNow;
        }

        public ChatMessageProcessorSelector(SocketViewerWebSocketBridge socketViewerWebSocketBridge, DiscordService discordService,
            IMemory memory, ILLMClient llmClient, ITranslation translation, IMovement movement, IVoiceSynthesis voice, ISpeechRecognition speechRecognition)
        {
            _socketViewerWebSocketBridge = socketViewerWebSocketBridge;
            _discordService = discordService;
            _memory = memory;
            _llmClient = llmClient;
            _translation = translation;
            _movement = movement;
            _voice = voice;
            _timer = new Timer(Execute, null, TimeSpan.Zero, _interval);
            _speechRecognition = speechRecognition;
            //_speechRecognition.Start();
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
                var cts = new CancellationTokenSource();
                CancellationToken cancellationToken = cts.Token;

                _memory.StartProcessing();

                var chatMessagesToProcess = _memory.GetChatMessagesToProcess();

                var aiAnswer = await _llmClient.GenerateAsync(chatMessagesToProcess);

                aiAnswer = aiAnswer
                    .Replace("*", string.Empty);

                _memory.AddEntities(chatMessagesToProcess);

                _memory.AddEntity("LeporaAI", aiAnswer);

                var userMessages = string.Join(" :", chatMessagesToProcess.Select(m => $"{m.Role}:{m.Message}"));

                Debug.WriteLine($"Users messages :{userMessages}");
                Console.WriteLine($"Users messages :{userMessages}");
                Console.WriteLine($"Ai answer:{aiAnswer}");
                Debug.WriteLine($"Ai answer:{aiAnswer}");
                //_movement.StartWinkAnimation();
                //_movement.OpenMouth();

                //await _socketViewerWebSocketBridge.SendMessageAsync(aiAnswer);//TODO: UNCOMMIT AND START TO WORK WITH IT IN DISCORD
                _voice.Speak(aiAnswer);
                //var speakAudioPath = _voice.GenerateSpeakAudioAndGetFilePath(aiAnswer);

                //CurrentSpeakAudioPath = speakAudioPath;

                //var audioDuration = _voice.GetAudioDuration(speakAudioPath);

                //var speeachMessage = new SpeeachMessage()
                //{ AiAnswer = aiAnswer, CurrentSpeakAudioPath = speakAudioPath, UserMessages = userMessages };


                //await _speechRecognition.SendMessageAsync(JsonSerializer.Serialize(speeachMessage));

                //await Task.Delay(TimeSpan.FromSeconds(audioDuration.Seconds), cancellationToken);

                //var synthesizer = new SpeechSynthesizer();//TODO REWRITE IT TO ANOTHER TTS!!!!!!!!!!!!!

                //synthesizer.SelectVoice("Microsoft Irina Desktop");

                //synthesizer.Speak($"{aiAnswer}");
                //synthesizer.Speak($"{messageToProcess.Role} {aiAnswer}");

                //_idleTimer.Change(_idleInterval, Timeout.InfiniteTimeSpan);//Todo think about deleting idle timer 

                //_movement.CloseMouth();

                _memory.StopProcessing();
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }

    public class SpeeachMessage()//TODO MOVE TO FOLDER 
    {
        public string? CurrentSpeakAudioPath { get; set; }

        public string? UserMessages { get; set; }

        public string? AiAnswer { get; set; }
    }
}
