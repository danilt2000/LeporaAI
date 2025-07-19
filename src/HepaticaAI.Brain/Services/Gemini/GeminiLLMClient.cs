using HepaticaAI.Brain.Models;
using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Models.Messages;
using Microsoft.Extensions.Configuration;
using System.Speech.Synthesis;

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

        public async Task<string> GenerateAsync(List<MessageEntry> messages)
        {
            var savedMessages = memory.GetFormattedPrompt();

            var combinedText = string.Join(" :", messages.Select(m => $"{m.Role}:{m.Message}"));
            var result = await geminiApiService.SummarizeAsync(savedMessages + combinedText);

            memory.AddEntity("LeporaAI", result);
            return result;
        }
    }
}
