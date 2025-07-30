using HepaticaAI.Core.Interfaces.Memory;
using System.Text;
using HepaticaAI.Core.Models.Messages;
using System.Collections.Generic;

namespace HepaticaAI.Brain.Services
{
    internal class AIPromptsMemory : IMemory
    {

        private readonly List<MessageEntry> _history = new();

        internal readonly List<MessageEntry> _unprocessedChatMessagesQueue = new();

        internal readonly List<MessageEntry> _unprocessedVoiceChatMessagesQueue = new();

        //private static readonly List<string> DefaultStopSequence = new List<string>();
        private static readonly List<string> DefaultStopSequence = ["```", ")", "(", "[", "]", "**", "*", "\n", "\\n"];//Todo uncommit
        //private static readonly List<string> DefaultStopSequence = ["```", ")", "(", "[", "]", "**", "*"];//Todo uncommit

        private static List<string> _stopSequence = DefaultStopSequence;

        private bool _isProcessing = false;

        public void AddEntity(string role, string message)//Todo ADD PROCESSING OF STOP SEQUENCES FOR NEW PEOPLE IN MEMORY
        {
            _history.Add(new MessageEntry(role, message));
        }

        public void AddEntities(List<MessageEntry> messages)
        {
            _history.AddRange(messages);
        }

        public void AddUserRoleToStopSequenceIfMissing(string role)
        {
            if (_stopSequence.Contains($"{role}:") || _stopSequence.Contains($"\n{role}:"))
                return;

            _stopSequence.Add($"{role}:");

            _stopSequence.Add($"\n{role} ");
        }

        public List<string> GetStopSequence()
        {
            return _stopSequence;
        }

        public void DeleteLastMessage()
        {
            _history.RemoveAt(_history.Count - 1);
        }

        public void AddEntryToProcessInQueue(string role, string message)
        {
            _unprocessedChatMessagesQueue.Add(new MessageEntry(role, message));
        }

        public void AddEntitiesToProcessInQueue(List<MessageEntry> messages)
        {
            _unprocessedChatMessagesQueue.AddRange(messages);
        }

        public void AddVoiceEntryToProcessInQueue(string role, string message)
        {
            _unprocessedVoiceChatMessagesQueue.Add(new MessageEntry(role, message));
        }

        public string GetFormattedPrompt()
        {
            var sb = new StringBuilder();

            int startIndex = Math.Max(0, _history.Count - 20);
            //int startIndex = Math.Max(0, _history.Count - 14);

            for (int i = startIndex; i < _history.Count; i++)
            {
                string cleanMessage = _history[i].Message.Replace("\r", "").Trim();

                if (i == startIndex)
                {
                    sb.Append($"{_history[i].Role}: {cleanMessage}");
                }
                else
                {
                    sb.Append($"\n{_history[i].Role}: {cleanMessage}");
                }
            }

            return sb.ToString();
        }

        public List<MessageEntry> GetHistoryMessages()
        {
            List<MessageEntry> messages = new List<MessageEntry>();

            int startIndex = Math.Max(0, _history.Count - 25);
            //int startIndex = Math.Max(0, _history.Count - 14);

            for (int i = startIndex; i < _history.Count; i++)
            {
                string cleanMessage = _history[i].Message.Replace("\r", "").Trim();

                if (i == startIndex)
                {
                    messages.Add(new MessageEntry(_history[i].Role, cleanMessage));
                }
                else
                {
                    messages.Add(new MessageEntry(_history[i].Role, cleanMessage));
                }
            }

            return messages;
        }

        public string GetFormattedPromptWithoutMemoryForgeting()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < _history.Count; i++)
            {
                string cleanMessage = _history[i].Message.Replace("\r", "").Trim();

                if (i == 0)
                {
                    sb.Append($"{_history[i].Role}: {cleanMessage}");
                }
                else
                {
                    sb.Append($"\n{_history[i].Role}: {cleanMessage}");
                }
            }

            return sb.ToString();
        }

        public void Clear()
        {
            _history.Clear();

            _unprocessedChatMessagesQueue.Clear();

            _unprocessedVoiceChatMessagesQueue.Clear();

            _stopSequence = DefaultStopSequence;
        }

        public bool HasMessagesToProcess()
        {
            return _unprocessedChatMessagesQueue.Count != 0;
        }

        public bool HasVoiceMessagesToProcess()
        {
            return _unprocessedVoiceChatMessagesQueue.Count != 0;
        }

        public bool IsNotCurrentlyProcessingMessage()
        {
            return !_isProcessing;
        }

        public void StartProcessing()
        {
            _isProcessing = true;
        }

        public void StopProcessing()
        {
            _isProcessing = false;
        }

        public MessageEntry GetMessageToProcess()
        {
            var entity = _unprocessedChatMessagesQueue.First();

            _unprocessedChatMessagesQueue.RemoveAt(0);

            return new MessageEntry(entity.Role, entity.Message);
        }

        public List<MessageEntry> GetChatMessagesToProcess()
        {
            var entities = new List<MessageEntry>(_unprocessedChatMessagesQueue);

            _unprocessedChatMessagesQueue.Clear();

            return entities;
        }

        public List<MessageEntry> GetVoiceMessagesToProcess()
        {
            var entities = new List<MessageEntry>(_unprocessedVoiceChatMessagesQueue);

            _unprocessedVoiceChatMessagesQueue.Clear();

            return entities;
        }
    }
}
