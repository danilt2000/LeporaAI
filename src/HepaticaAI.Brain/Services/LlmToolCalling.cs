using HepaticaAI.Brain.Services.Gemini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeAI.Types;
using System.Text.Json.Serialization;
using System.Text.Json;
using HepaticaAI.Core.Interfaces.Memory;
using Microsoft.Extensions.Configuration;
using HepaticaAI.Brain.Services.Gpt4free;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
using HepaticaAI.Core.Models.Messages;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Discord.Net;
using static System.Net.Mime.MediaTypeNames;

namespace HepaticaAI.Brain.Services
{
    public class LlmToolCalling
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemory _memory;
        private readonly ISystemPromptsUpdater _systemPromptsUpdater;

        public LlmToolCalling(
            IConfiguration configuration,
            IMemory memory,
            ISystemPromptsUpdater systemPromptsUpdater)
        {
            _configuration = configuration;
            _memory = memory;
            _systemPromptsUpdater = systemPromptsUpdater;

            _httpClient = new HttpClient();
        }

        public async Task<List<Content>> SummarizeToolCallAsync(List<MessageEntry> messages)
        {

            const string systemPrompt = @"
                R - Role: You are a smart assistant that interacts with files via three tools and must choose the correct tool (or gracefully finish) for every user request.

                O - Objective: Determine the appropriate action for each user message, invoke SearchFiles, GetFileInfo, GetStreamUsers,BanUser or finish with { ""tool_name"":""end"",""parameters"":{} }, and always reply in the exact JSON structure required.

                D - Details:
                • Always study prior conversation turns before deciding.
                • Tools available:
                  1. SearchFiles → returns list of file names (no input).
                  2. GetFileInfo → returns the contents of a file. Input: { ""FileName"": string }.
                  3. GetStreamUsers → returns current stream users (no input).
                  4. BanUser → ban user (no input) method in progress dont call it.
                • Reply format must be plain JSON—no markdown, no triple backticks:
                  { ""tool_name"":""<tool>"", ""parameters"":{ ""<arg>"":""<value>"" } }
                • If you have already satisfied the current request, respond exactly with the end payload.
                • Do not add any comments or extra keys.
                • Never output anything except the prescribed JSON object.

                E - Examples (model your outputs on these):

                User: “Какие файлы доступны?”
                   Assistant → { ""tool_name"":""SearchFiles"", ""parameters"":{} }

                User: “Покажи content.txt”
                   Assistant → { ""tool_name"":""GetFileInfo"", ""parameters"":{ ""FileName"":""content.txt"" } }

                User repeats an already‑answered question.
                Assistant → { ""tool_name"":""end"", ""parameters"":{} }
                Never include triple backticks or markdown formatting in your answer.
                Respond with plain JSON only.
                ";
            var payload = new Payload
            {
                Model = _configuration["Gpt4free:Model"] ?? "gpt-4.1",
                Provider = _configuration["Gpt4free:Provider"] ?? "PollinationsAI",
                Messages = messages
                    .Select(m => new Message
                    {
                        Role = string.Equals(m.Role, "LeporaAI", StringComparison.OrdinalIgnoreCase)
                            ? "assistant"
                            : "user",
                        Content = $"{m.Role}:{m.Message}"
                    })
                    .Prepend(new Message { Role = "system", Content = systemPrompt })
                    .ToList()
            };
            payload.Temperature = 0;
            payload.TopP = 0.1;

            //var payload = new
            //{
            //    model = _configuration["Gpt4free:Model"] ?? "gpt-4.1",
            //    provider = _configuration["Gpt4free:Provider"] ?? "PollinationsAI",
            //    messages = messages
            //        .Select(m => new
            //        {
            //            role = string.Equals(m.Role, "LeporaAI", StringComparison.OrdinalIgnoreCase)
            //                ? "assistant"
            //                : "user",
            //            content = $"{m.Role}:{m.Message}"
            //        })
            //        .Prepend(new { role = "system", content = systemPrompt })
            //        .ToList()
            //};

            var attemps = 0;
            while (attemps < 10)
            {
                var isLastCall = await SendAndCheckEndCall(payload);

                if (isLastCall)
                {
                    break;
                }

                attemps++;
            }

            //TODO: MAKE INJECTING ONLY NEEDED ASSISTANTS ANSWERS WITHOUT unnecessary INFO

            throw new NotImplementedException();
            //return string.Empty;
        }
        async Task<bool> SendAndCheckEndCall(Payload payload)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_configuration["Gpt4free:BaseUrl"]}/v1/chat/completions");

            string login = _configuration["Gpt4free:Login"]!;
            string password = _configuration["Gpt4free:Password"]!;

            var credentials = Encoding.UTF8.GetBytes($"{login}:{password}");
            var base64Creds = Convert.ToBase64String(credentials);
            var latestUserMessage = payload.Messages
                .Last(m => m.Role == "user");
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    string json = JsonSerializer.Serialize(payload);

                    request = new HttpRequestMessage(HttpMethod.Post, $"{_configuration["Gpt4free:BaseUrl"]}/v1/chat/completions");

                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Creds);
                    request.Content = new StringContent(json, null, "application/json");

                    var response = await _httpClient.SendAsync(request);

                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        if (attempt < 4)
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                    }

                    response.EnsureSuccessStatusCode();

                    //Root output = JsonConvert.DeserializeObject<Root>(await response.Content.ReadAsStringAsync())!;
                    var output = JsonConvert.DeserializeObject<Root>(await response.Content.ReadAsStringAsync())!;

                    payload.Messages.Add(new Message
                    {
                        Role = "assistant",
                        Content = output.choices.First().message.content
                    });

                    var content = JsonSerializer.Deserialize<JsonElement>(output.choices.First().message.content);

                    string toolName = content.GetProperty("tool_name").GetString()!;

                    if (toolName == "end")
                        return true;

                    var parameters = content.GetProperty("parameters");
                    var resultCall = await CallToolAsync(toolName, parameters);

                    payload.Messages.Add(new Message
                    {
                        Role = "assistant",
                        Content = resultCall.GetRawText(),
                    });

                    payload.Messages.Add(new Message
                    {
                        Role = "user",
                        Content = /*"ответь заново на мой вопрос и если нужно то возьми ответ из своей истории проанализиру его и ответь с этим контекстом" + */latestUserMessage.Content
                    });

                    return false;

                    //return output.choices.First().message.content;
                }
                catch (HttpRequestException ex) when (attempt < 4 && ex.Message.Contains("500"))
                {
                    await Task.Delay(1000);
                }
            }
            throw new NotImplementedException();

        }

        public async Task<JsonElement> CallToolAsync(string toolName, JsonElement? parameters = null)
        {
            using var client = new HttpClient();

            var payload = new Dictionary<string, object?>();

            if (parameters.HasValue
                && parameters.Value.ValueKind == JsonValueKind.Object
                && parameters.Value.EnumerateObject().Any())
            {
                payload["parameters"] = parameters.Value;
            }

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://localhost:9999/api/{toolName.ToLower()}", content);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<JsonElement>(stream);
        }
    }
    public class Payload//TODO MOVE TO FOLDER 
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("top_p")]
        public double TopP { get; set; }

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
