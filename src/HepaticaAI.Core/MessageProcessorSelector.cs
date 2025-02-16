using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.Movement;
using HepaticaAI.Core.Interfaces.Translations;
using HepaticaAI.Core.Interfaces.Voice;
using HepaticaAI.Core.Models.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using HepaticaAI.Core.Models;

namespace HepaticaAI.Core
{
    public class MessageProcessorSelector : IDisposable
    {
        private readonly IMemory _memory;
        private readonly ILLMClient _llmClient;
        private readonly ITranslation _translation;
        private readonly IMovement _movement;
        private readonly IVoiceSynthesis _voice;
        private readonly Timer _timer;
        private readonly SemaphoreSlim _semaphore = new(5);
        private readonly ConcurrentQueue<(Guid, MessageEntry)> _messageQueue = new();
        private readonly ConcurrentDictionary<Guid, MessageForVoiceToProcess> _generatedResponses = new();
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);

        public MessageProcessorSelector(IMemory memory, ILLMClient llmClient, ITranslation translation, IMovement movement, IVoiceSynthesis voice)
        {
            _memory = memory;
            _llmClient = llmClient;
            _translation = translation;
            _movement = movement;
            _voice = voice;
            _timer = new Timer(EnqueueMessages, null, TimeSpan.Zero, _interval);
            Task.Run(ProcessMessagesAsync);
            Task.Run(SpeakNextMessage);
        }

        private void EnqueueMessages(object? state)
        {
            while (_memory.HasMessagesToProcess())
            {
                var messageToProcess = _memory.GetMessageToProcess();
                var messageId = Guid.NewGuid();
                _messageQueue.Enqueue((messageId, messageToProcess));
            }
        }

        private async Task ProcessMessagesAsync()
        {
            while (true)
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (_messageQueue.TryDequeue(out var message))
                    {
                        var (messageId, messageToProcess) = message;

                        Debug.WriteLine($"Processing message {messageId}: {messageToProcess.Message}");

                        string aiAnswer =
                            await _llmClient.GenerateAsync(messageToProcess.Role, messageToProcess.Message);

                        if (!IsMostlyRussian(aiAnswer))
                            aiAnswer = await _translation.TranslateEngtoRu(aiAnswer);

                        _generatedResponses[messageId] = new MessageForVoiceToProcess()
                        { AiMessage = aiAnswer, UserMessage = messageToProcess.Message };

                        Debug.WriteLine($"Generated AI response {messageId}: {aiAnswer}");
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
                foreach (var messageId in _generatedResponses.Keys)
                {
                    if (_generatedResponses.TryRemove(messageId, out var aiAnswer))
                    {
                        Debug.WriteLine($"Speaking message {messageId}: {aiAnswer}");

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