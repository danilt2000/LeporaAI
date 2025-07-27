using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Models.Messages;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HepaticaAI.Brain.Services.Gpt4free
{
    public class Gpt4freeLLMClient : ILLMClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemory _memory;
        private readonly ISystemPromptsUpdater _systemPromptsUpdater;

        public Gpt4freeLLMClient(
            IConfiguration configuration,
            IMemory memory,
            ISystemPromptsUpdater systemPromptsUpdater)
        {
            _configuration = configuration;
            _memory = memory;
            _systemPromptsUpdater = systemPromptsUpdater;

            _httpClient = new HttpClient();
        }

        public void Dispose() => _httpClient.Dispose();

        public void Initialize()
        {
        }

        public async Task<string> GenerateAsync(string personality, string prompt)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GenerateAsync(List<MessageEntry> messages)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_configuration["Gpt4free:BaseUrl"]}/v1/chat/completions");
                string login = _configuration["Gpt4free:Login"]!;
                string password = _configuration["Gpt4free:Password"]!;
                var payload = new
                {
                    model = _configuration["Gpt4free:Model"] ?? "gpt-4.1",
                    provider = _configuration["Gpt4free:Provider"] ?? "PollinationsAI",
                    messages = messages
                        .Select(m => new { role = m.Role, content = m.Message })
                        .ToList()
                };

                string json = JsonSerializer.Serialize(payload);

                var credentials = Encoding.UTF8.GetBytes($"{login}:{password}");
                var base64Creds = Convert.ToBase64String(credentials);

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Creds);
                request.Content = new StringContent(json, null, "application/json");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
