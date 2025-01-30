using HepaticaAI.Core.Interfaces.Memory;
using System.Text;

namespace HepaticaAI.Brain.Services
{
        internal class AIPromptsMemory : IMemory
        {
                private readonly List<(string Role, string Message)> _history = new();

                public void AddEntry(string role, string message)//Todo ADD PROCESSING OF STOP SEQUENCES FOR NEW PEOPLE IN MEMORY
                {
                        _history.Add((role, message));
                }

                public string GetFormattedPrompt()
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
                }
        }
}
