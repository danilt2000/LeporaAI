using HepaticaAI.Core.Models.Messages;

namespace HepaticaAI.Core.Interfaces.AI
{
    public interface ILLMClient : IDisposable
    {
        void Initialize();

        Task<string> GenerateAsync(string personality, string prompt);

        Task<string> GenerateAsync(List<MessageEntry> messages);
    }
}
