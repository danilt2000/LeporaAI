using HepaticaAI.Core.Interfaces.Memory;
using System.Text;
using HepaticaAI.Core.Models.Messages;

namespace HepaticaAI.Brain.Services
{
        internal class AIPromptsMemory : IMemory
        {
                private readonly List<(string Role, string Message)> _history = new();
                internal readonly List<(string Role, string Message)> _unprocessedMessagesQueue = new();
                internal readonly List<string> _stopSequence = ["```", ")", "(", "[", "]"];

                private bool _isProcessing = false;

                public void AddEntry(string role, string message)//Todo ADD PROCESSING OF STOP SEQUENCES FOR NEW PEOPLE IN MEMORY
                {
                        _history.Add((role, message));
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

                public void AddEntryToProcessInQueue(string role, string message)//Todo ADD PROCESSING OF STOP SEQUENCES FOR NEW PEOPLE IN MEMORY
                {
                        _unprocessedMessagesQueue.Add((role, message));
                }

                public string GetFormattedPrompt()
                {
                        var sb = new StringBuilder();

                        int startIndex = Math.Max(0, _history.Count - 12);

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

                public void Clear()//Todo if it's not used delete it 
                {
                        _history.Clear();
                }

                public bool HasMessagesToProcess()
                {
                        return _unprocessedMessagesQueue.Count != 0;
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
                        var entity = _unprocessedMessagesQueue.First();

                        _unprocessedMessagesQueue.RemoveAt(0);

                        return new MessageEntry(entity.Role, entity.Message);
                }
        }
}
