using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Models.Messages;
using Microsoft.Extensions.Configuration;

namespace HepaticaAI.Brain.Services.Gemini
{
    internal class GeminiLLMClient(GeminiApiService geminiApiService, IConfiguration configuration, IMemory memory, ISystemPromptsUpdater systemPromptsUpdater) : ILLMClient
    {
        public void Dispose()
        {

        }

        public void Initialize()
        {
        }

        public async Task<string> GenerateAsync(string personality, string prompt)
        {
            var result = await geminiApiService.SummarizeAsync(personality + prompt);

            return result;
        }

        public Task<string> GenerateAsync(List<MessageEntry> messages)
        {
            throw new NotImplementedException();
        }
    }
}
