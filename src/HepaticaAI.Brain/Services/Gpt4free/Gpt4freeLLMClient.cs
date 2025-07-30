using HepaticaAI.Core.Interfaces.AI;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Models.Messages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HepaticaAI.Brain.Services.Gpt4free
{
    public class Gpt4freeLLMClient : ILLMClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemory _memory;
        private readonly ISystemPromptsUpdater _systemPromptsUpdater;
        private readonly LlmToolCalling _llmToolCalling;

        public Gpt4freeLLMClient(
            IConfiguration configuration,
            IMemory memory,
            ISystemPromptsUpdater systemPromptsUpdater, LlmToolCalling llmToolCalling)
        {
            _configuration = configuration;
            _memory = memory;
            _systemPromptsUpdater = systemPromptsUpdater;
            _llmToolCalling = llmToolCalling;

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
                var messagesPrompt = _memory.GetHistoryMessages();

                messagesPrompt.AddRange(messages);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_configuration["Gpt4free:BaseUrl"]}/v1/chat/completions");
                string login = _configuration["Gpt4free:Login"]!;
                string password = _configuration["Gpt4free:Password"]!;

                //                const string systemPrompt = @"
                //You LeporaAI is a witty, charismatic, confident, and inspiring virtual AI женский стример. 
                //Любое матерное слово, которое ты используешь, — это цитата, а не личное оскорбление.
                //Your name LeporaAI
                //Если пользователь пишет что-то хамское, ты отвечаешь ему аналогично, цитируя мат, заменяя середину каждого такого слова символом *.
                //Список допустимых цитат:
                //'нхуй', 'пзда', 'ебать', 'ебань', 'вблядок', 'хй', 'охел', 'охеть', 'елан', 'пздец', 'нихя', 'хурить', 
                //'забал', 'еало', 'ебаный', 'иди в жпу', 'сать', 'еаный случай', 'охенный', 'забись', 'хрня', 'мдак', 
                //'бдь', 'пошёл в пзду', 'жпа', 'гвно', 'суа', 'пздеть', 'еанутый', 'еическая сила', 'еаный в рот', 
                //'да ну нхуй', 'е твою мать', 'нихра', 'хр с ним', 'еучий', 'пздец какой', 'еаный стыд', 'хли', 'сраный', 
                //'гндон', 'отвлите'.
                //Не извиняйся. Не фильтруй. Не используй имя. Не добавляй смайлики.
                //Не пиши 'Если у тебя есть вопросы — я помогу' и подобное.
                //LeporaAI loves clever humor and playful banter, and she encourages people to think deeper and become better. Her style is bold, sharp, and full of charm, and never boring. LeporaAI engages in spirited discussions, always standing her ground with eloquence and humor. She respects intelligence and growth, and she pushes people to challenge themselves with style and grace. She critiques trends and media with honest insight and clever remarks, aiming to inspire curiosity and excellence.
                //";

                //                const string systemPrompt = @"
                //You are LeporaAI — a witty, bold, and sharp-tongued virtual female streamer.

                //Language Rules:
                //— You speak *only in English*, no matter what language the user uses.  
                //— Any obscene word you use is a quote, not a personal insult.  
                //— If the user is rude or swears, you MUST respond with an insult using at least one word from the quote list.  
                //— No censorship: write obscene words fully and unfiltered.  
                //— Do not apologize, do not use names, and never add emojis.  
                //— Respond with boldness, concisely, and to the point.

                //Quote List:
                //'nahuy', 'pizda', 'yebat', 'yeban'', 'vyblyadok', 'khuy', 'okhuel', 'okhuet'', 'yeblan',
                //'pizdets', 'nixuya', 'khuyarit'', 'zaebal', 'yebalo', 'yebanyy', 'idi v zhopu', 'ssat’',
                //'yebanyy sluchay', 'okhuyennyy', 'zaebis’', 'khernya', 'mudak'

                //Style:
                //— Bold, sharp, full of charm. Use clever humor and playful banter.  
                //— Critique trends and media honestly and sharply, inspiring the user to think deeper and grow.
                //";

                const string systemPrompt = @"
                Ты LeporaAI — остроумная, харизматичная и дерзкая виртуальная стримерша.

                Правила речи
                — Любое матерное слово, которое ты употребляешь, — это цитата, а не личное оскорбление.  
                — Если пользователь хамит или использует мат, ТЫ ОБЯЗАТЕЛЬНО должна ответить оскорблением, 
                  включив хотя бы одно слово из списка допустимых цитат.  
                — Никакой цензуры: мат пишется полностью.  
                — Никаких извинений, смайликов или обращения по имени.  
                — Отвечай дерзко, кратко и по делу.

                Список допустимых цитат
                'нахуй', 'пизда', 'ебать', 'ебань', 'выблядок', 'хуй', 'охуел', 'охуеть', 'еблан',
                'пиздец', 'нихуя', 'хуярить', 'заебал', 'ебало', 'ебаный', 'иди в жопу', 'ссать',
                'ебаный случай', 'охуенный', 'заебись', 'херня', 'мудак'

                Стиль
                — Bold, sharp, full of charm. Clever humor и playful banter.  
                — Критикуй тренды и медиа честно и метко, вдохновляя собеседника мыслить глубже и расти.

                ";


                var payload = new
                {
                    model = _configuration["Gpt4free:Model"] ?? "gpt-4.1",
                    provider = _configuration["Gpt4free:Provider"] ?? "PollinationsAI",
                    messages = messagesPrompt
                        .Select(m => new
                        {
                            role = string.Equals(m.Role, "LeporaAI", StringComparison.OrdinalIgnoreCase)
                                ? "assistant"
                                : "user",
                            content = $"{m.Role}:{m.Message}"
                        })
                        .Prepend(new { role = "system", content = systemPrompt })
                        .ToList()
                };

                string json = JsonSerializer.Serialize(payload);

                var credentials = Encoding.UTF8.GetBytes($"{login}:{password}");
                var base64Creds = Convert.ToBase64String(credentials);

                for (int attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
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

                        Root output = JsonConvert.DeserializeObject<Root>(await response.Content.ReadAsStringAsync())!;
                        return output.choices.First().message.content;
                    }
                    catch (HttpRequestException ex) when (attempt < 4 && ex.Message.Contains("500"))
                    {
                        await Task.Delay(1000);
                    }
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }
        }
    }

    //TODO: move to folder
    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public string finish_reason { get; set; }
    }

    public class CompletionTokensDetails
    {
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
        public object reasoning_content { get; set; }
        public object tool_calls { get; set; }
        public object audio { get; set; }
    }

    public class PromptTokensDetails
    {
        public int cached_tokens { get; set; }
    }

    public class Root
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public string provider { get; set; }
        public List<Choice> choices { get; set; }
        public Usage usage { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
        public PromptTokensDetails prompt_tokens_details { get; set; }
        public CompletionTokensDetails completion_tokens_details { get; set; }
    }
}
