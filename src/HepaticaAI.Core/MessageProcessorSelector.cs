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
using System.Text.RegularExpressions;

namespace HepaticaAI.Core
{
    public class MessageProcessorSelector : IDisposable
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
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);

        public bool IsNotPlayingIntermediateOrFinalSpeech { get; set; } = true;

        public DateTime LastIsNotPlayingIntermediateOrFinalSpeechChange { get; set; } = DateTime.MinValue;

        public void SetFalseIsNotPlayingIntermediateSpeech()
        {
            IsNotPlayingIntermediateOrFinalSpeech = false;

            LastIsNotPlayingIntermediateOrFinalSpeechChange = DateTime.UtcNow;
        }

        public MessageProcessorSelector(IMemory memory, ILLMClient llmClient, ITranslation translation, IMovement movement, IVoiceSynthesis voice/*, ISpeechRecognition speechRecognition*/)
        {
            _memory = memory;
            _llmClient = llmClient;
            _translation = translation;
            _movement = movement;
            _voice = voice;
            //_speechRecognition = speechRecognition;
            //_timer = new Timer(EnqueueMessages, null, TimeSpan.Zero, _interval);
            //_timer = new Timer(EnqueueVoiceMessages, null, TimeSpan.Zero, _interval);
            //_timer = new Timer(UpdateIsNotPlayingIntermediateOrFinalSpeechState, null, TimeSpan.Zero, _interval);
            _timer = new Timer(TimerCallback, null, TimeSpan.Zero, _interval);

            Task.Run(ProcessMessagesAsync);
            Task.Run(SpeakNextMessage);
        }

        public void Start()
        {

        }

        private void TimerCallback(object? state)
        {
            EnqueueMessages(state);
            EnqueueVoiceMessages(state);
            UpdateIsNotPlayingIntermediateOrFinalSpeechState(state);
        }

        private void UpdateIsNotPlayingIntermediateOrFinalSpeechState(object? state)
        {
            if ((DateTime.UtcNow - LastIsNotPlayingIntermediateOrFinalSpeechChange).TotalSeconds > 3)
                IsNotPlayingIntermediateOrFinalSpeech = true;
            else
                IsNotPlayingIntermediateOrFinalSpeech = false;
        }

        private void EnqueueVoiceMessages(object? state)
        {
            if (_memory.HasVoiceMessagesToProcess())
            {
                var voiceMessagesToProcess = new List<MessageEntry>(_memory.GetVoiceMessagesToProcess());
                _voiceChatMessageQueue.AddRange(voiceMessagesToProcess);
            }
        }

        private void EnqueueMessages(object? state)
        {
            if (_memory.HasMessagesToProcess())//Todo test it 
            {
                var chatMessageToProcess = _memory.GetMessageToProcess();
                var chatMessageId = Guid.NewGuid();
                _chatMessageQueue.Enqueue((chatMessageId, chatMessageToProcess));
            }
        }

        private async Task ProcessMessagesAsync()
        {
            while (true)
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (_chatMessageQueue.TryDequeue(out var message))
                    {
                        var (messageId, messageToProcess) = message;

                        Debug.WriteLine($"Processing message {messageId}: {messageToProcess.Message}");

                        string aiAnswer = await _llmClient.GenerateAsync(messageToProcess.Role, messageToProcess.Message);

                        if (!IsMostlyRussian(aiAnswer))
                            aiAnswer = await _translation.TranslateEngtoRu(aiAnswer);

                        _generatedChatResponses[messageId] = new MessageForVoiceToProcess()
                        { AiMessage = aiAnswer, UserMessage = messageToProcess.Message };

                        Debug.WriteLine($"Generated AI response {messageId}: {aiAnswer}");
                    }
                    else if (_voiceChatMessageQueue.Count != 0 && IsNotPlayingIntermediateOrFinalSpeech)
                    {
                        var allVoiceMessages = new List<MessageEntry>(_voiceChatMessageQueue);

                        _voiceChatMessageQueue.Clear();

                        //TODO IMPLEMENT VOICE CHAT SPEAK VIA DISCORD AND IMPLEMENT GENERATE ASYNC FUNCTION 
                        var aiAnswer = await _llmClient.GenerateAsync(allVoiceMessages);

                        if (!IsMostlyRussian(aiAnswer))
                            aiAnswer = await _translation.TranslateEngtoRu(aiAnswer);

                        //var speakAudioPath = _voice.GenerateSpeakAudioAndGetFilePath("TEST TEST TEST");
                        var speakAudioPath = _voice.GenerateSpeakAudioAndGetFilePath(aiAnswer);

                        CurrentSpeakAudioPath = speakAudioPath;
                        //await _speechRecognition.SendMessageAsync("TEST TEST TEST");

                        Debug.WriteLine($"Generated AI response");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    _semaphore.Release();
                }

                await Task.Delay(500);
            }
        }

        public void SpeakNextMessage()
        {
            while (true)
            {
                foreach (var messageId in _generatedChatResponses.Keys)
                {
                    if (_generatedChatResponses.TryRemove(messageId, out var aiAnswer))
                    {
                        _movement.StartWinkAnimation();
                        _movement.OpenMouth();
                        Debug.WriteLine($"User message {aiAnswer.UserMessage}: ai answer {aiAnswer.AiMessage}");
                        _voice.Speak(aiAnswer.AiMessage!);
                        _movement.CloseMouth();
                    }
                }
            }
        }

        public static bool IsMostlyRussian(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            int russianCount = Regex.Matches(text, "[а-яА-ЯёЁ]").Count;
            int englishCount = Regex.Matches(text, "[a-zA-Z]").Count;

            return russianCount >= englishCount;
        }

        public void Dispose()
        {
            _timer.Dispose();
            _semaphore.Dispose();
        }
    }
}