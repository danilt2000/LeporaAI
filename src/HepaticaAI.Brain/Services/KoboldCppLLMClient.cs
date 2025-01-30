using HepaticaAI.Brain.Models;
using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace HepaticaAI.Brain.Services
{
        internal class KoboldCppLLMClient(KoboldCppRunner koboldCppRunner, IConfiguration configuration, IMemory memory) : ILLMClient
        {
                public void Initialize()
                {
                        koboldCppRunner.StartKoboldCpp();
                }

                public async Task<string> GenerateAsync(string personality, string prompt)
                {
                        memory.AddEntry(personality, prompt);

                        var characterPersonality = JsonSerializer.Deserialize<Personality>(await File.ReadAllTextAsync("character_personality.json"));

                        var aiUrl = configuration["AiUrl"];
                        var aiMainCharacterName = configuration["AiMainCharacterName"];

                        memory.AddEntry(aiMainCharacterName!, string.Empty);

                        characterPersonality!.prompt = memory.GetFormattedPrompt();

                        var jsonBody = JsonSerializer.Serialize(characterPersonality, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        using var client = new HttpClient();

                        var request = new HttpRequestMessage(HttpMethod.Post, $"{aiUrl}/api/v1/generate")
                        {
                                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                        };

                        var response = await client.SendAsync(request);

                        var responseContent = await response.Content.ReadAsStringAsync();

                        return responseContent;
                }

                public void Dispose()
                {
                        koboldCppRunner.StopKoboldCpp();
                }
        }
}
