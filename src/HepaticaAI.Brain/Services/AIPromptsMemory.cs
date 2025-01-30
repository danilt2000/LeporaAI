using HepaticaAI.Core.Interfaces.Memory;
using System.Text;

namespace HepaticaAI.Brain.Services
{
        internal class AIPromptsMemory : IMemory
        {
                private readonly List<(string Role, string Message)> _history = new();

                public void AddEntry(string role, string message)
                {
                        _history.Add((role, message));
                }

                public string GetFormattedPrompt()
                {
                        var sb = new StringBuilder();
                        foreach (var entry in _history)
                        {
                                if (_history.Count == 0)
                                {
                                        sb.AppendLine($"{entry.Role}: {entry.Message}");

                                        continue;
                                }

                                sb.AppendLine($"\n{entry.Role}: {entry.Message}");
                        }

                        return sb.ToString().TrimEnd();
                }

                public void Clear()
                {
                        _history.Clear();
                }
        }
}
