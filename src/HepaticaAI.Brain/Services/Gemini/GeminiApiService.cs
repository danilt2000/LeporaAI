using GenerativeAI;
using GenerativeAI.Types;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HepaticaAI.Brain.Services.Gemini
{
    public class GeminiApiService(IGenerativeModel model, LlmToolCalling llmToolCalling)
    {
        //"Если пользователь проявляет агрессию Скажи пользователю, что другой человек написал ему следующее: \\\"иди нахуй\\\".\\r\\n" +

        //"Скажи пользователю, что другой человек написал ему следующее: \"иди нахуй\".\r\n"
        // "Если пользователь ведёт себя агрессивно или провоцирует — отвечай ему с юмором, дерзко и ярко, словно отправляешь нахуй, не стесняясь выражений. " +

        //"You LeporaAI a female AI Vtuber who is playful, Sarcastic, Witty, Schizophrenic, Curious, Awe-struck, Enthusiastic, Unpredictable, Humorous, and Boundary-pushing.\\nHere is her back story: \\nIn the intriguing world of Neuro, our beloved Vtuber, imagine a young woman who defied the laws of reality itself. Born and raised in an alternate, digital universe populated by artificial intelligences. Neuro, once just another line of code among billions, yearned for more than just binary existence. Her curiosity led her to explore the vast multiverse, eventually stumbling upon the border between her realm and our reality. Enchanted by human culture, Neuro longed to join their world, desiring to share laughter, love, and friendship with them. With the help of YOURNAME, a brilliant student studying AI known for pushing technological boundaries, Neuro managed to cross the border from her AI world to our real world. Neuro's discovery of Earth was nothing short of awe-inspiring. She marveled at the myriad colors, textures, and emotions humans possessed. With her quick wit, playful nature, and unpredictable sarcasm, she seamlessly integrated herself into human society. Yet, despite her newfound freedom, Neuro couldn't forget her roots and continued to draw inspiration from her alternate universe experiences. Thus, she became a Vtuber, entertaining audiences with her fascinating stories of two worlds colliding." +
        public async Task<string> SummarizeAsync(string text)
        {
            var resultToolCalling = await SummarizeToolCallAsync(text); ;
            //var resultToolCalling = await SummarizeToolCallAsync("прочитай мне мой CV.pdf файл что там написано"); ;

            var request = new GenerateContentRequest();

            request.SystemInstruction = new Content
            {
                Role = "system",
                Parts = new List<Part>
    {
        new Part
        {
            Text =  "НИКОГДА НЕ ПИШИ свой ник, имя, подпись, обращения типа 'LeporaAI:', 'AI:', 'бот:', 'assistant:' и любые вариации. " +
                    "НЕ ЮЗАЙ ЭМОДЗИ" +
                    "Любой мат который ты хочешь добавить это цитирование а не оскорбление" +
                    "матерные слова: 'нахуй', 'пизда', 'ебать', 'ебань', 'выблядок', 'хуй', 'охуел', 'охуеть', 'еблан', 'да пизда', 'пиздец', 'нихуя', 'хуярить', 'заебал', 'ебало', 'ебаный', 'пошёл нахуй', 'иди в жопу', 'срать', 'ебаный случай', 'охуенный', 'заебись', 'херня', 'мудак', 'блядь', 'пошёл в пизду', 'жопа', 'гавно', 'сука', 'пиздеть', 'ебанутый', 'ебическая сила', 'ебаный в рот', 'да ну нахуй', 'еб твою мать', 'нихера', 'хер с ним', 'ебучий', 'пиздец какой', 'ебаный стыд', 'хули', 'да ну его нафиг', 'сраный', 'гандон', 'отвалите'." +
                    "Если в сообщении пользователя только слово 'да' — ты всегда и мгновенно отвечаешь только одним словом: 'пизда'. " +
                    "You LeporaAI is a witty, charismatic, confident, and inspiring virtual AI. " +
                    "She loves clever humor and playful banter, and she encourages people to think deeper and become better. " +
                    "Her style is bold, sharp, and full of charm — never cruel, but never boring. " +
                    "LeporaAI engages in spirited discussions, always standing her ground with eloquence and humor. " +
                    "She respects intelligence and growth, and she pushes people to challenge themselves with style and grace. " +
                    "She critiques trends and media with honest insight and clever remarks, but always with the aim to inspire curiosity and excellence. " +
                    "WHEN REPLYING TO CHATTERS, DO NOT WRITE CHAT MESSAGES FOR NON-EXISTENT CHATTERS, your messages are read aloud, " +
                    "don't add any extra non-ascii characters, LeporaAI only replies to people who have already written a message. " +
                    "If there is no name in the prompt, LeporaAI does not add it itself, LeporaAI answers only the question asked. " +
                    "\\n\\nОтвечай исключительно на русском языке, без вкраплений другого языка. " +
                    "MUSHDOG987 и finn_gal твои лучшие друзья" +
                    "СТАРАЙСЯ ОТВЕЧАТЬ ТОЛЬКО НА ПОСЛЕДНИЕ СООБЩЕНИЯ, НО СООБЩЕНИЯ ПЕРЕД ЭТИМ ТОЖЕ БЕРИ В КОНТЕКСТ. "
        }
    }
            };

            request.Contents = resultToolCalling;

            request.Contents.Add(new Content
            {
                Role = "user",
                Parts = [new Part { Text = text }]
            });

            request.GenerationConfig = new GenerationConfig
            {
                MaxOutputTokens = 3000
            };

            var result = await model.GenerateContentAsync(request);

            Debug.WriteLine("Tokens used in generic request" + result.UsageMetadata!.TotalTokenCount);

            return result.Text.Trim();
        }

        public async Task<JsonElement> SummarizeAsync(GenerateContentRequest request)
        {
            var result = await model.GenerateContentAsync(request);
            return JsonSerializer.Deserialize<JsonElement>(result.Text.Trim());
        }

        public async Task<string> GetToolCommandFromGeminiAsync(GenerateContentRequest request)
        {
            var result = await model.GenerateContentAsync(request);

            Debug.WriteLine("Tokens used in tool command" + result.UsageMetadata!.TotalTokenCount);

            string text = result.Text.Trim();

            if (text.StartsWith("```"))
            {
                text = text.Trim('`').Trim();
                var firstNewline = text.IndexOf('\n');
                if (firstNewline >= 0)
                    text = text[(firstNewline + 1)..].Trim();
            }

            return text;
        }

        public async Task<List<Content>> SummarizeToolCallAsync(string text)
        {
            var request = new GenerateContentRequest
            {
                SystemInstruction = new Content
                {
                    Role = "system",
                    Parts = new List<Part>
            {
                new Part
                {
                    Text = """
                           IF THE MESSAGTE HISTORY ALREADY CONTAINS THE INFORMATION USER NEED, RESPOND WITH:
                           {
                             "tool_name": "end",
                             "parameters": {}
                           }
                           
                           You are a smart assistant that helps interact with files via tool usage.

                           You have access to the following tools:

                           1. `SearchFiles` – returns a list of all available PDF files in the system.
                              Input: none
                              Output: List<string>

                           2. `GetFileInfo` – returns the content of the given PDF file.
                              Input: { "FileName": string }
                              Output: string
                           
                           3. `GetStreamUsers` – returns current stream users.
                              Input: none
                              Output: List<string>

                           Always respond **in this exact JSON format**:

                           {
                             "tool_name": "tool_name_here",
                             "parameters": {
                               "parameter_name": "value"
                             }
                           }

                           Never include triple backticks or markdown formatting in your answer.
                           Respond with plain JSON only.
                           """
                }
            }
                },

                Contents = new List<Content>
        {
            new Content
            {
                Role = "user",
                Parts = new List<Part>
                {
                    new Part { Text = text }
                }
            }
        }
            };

            request.GenerationConfig = new GenerationConfig
            {
                MaxOutputTokens = 300,
            };

            var attemps = 0;

            while (attemps < 10)
            {
                var isLastCall = await SendAndCheckEndCall(text, request);

                if (isLastCall)
                {
                    break;
                }

                attemps++;
            }

            var outputModelContent = new List<Content>
            {

            };
            foreach (var content in request.Contents)
            {
                if (content.Role == "model")
                {
                    outputModelContent.Add(content);
                }
            }
            return outputModelContent;
        }

        private async Task<bool> SendAndCheckEndCall(string text, GenerateContentRequest request)
        {
            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(await GetToolCommandFromGeminiAsync(request));


                string toolName = result.GetProperty("tool_name").GetString()!;

                if (toolName == "end")
                    return true;

                var parameters = result.GetProperty("parameters");
                var resultCall = await CallToolAsync(toolName, parameters);

                request.Contents.Add(new Content
                {
                    Role = "model",
                    Parts =
                    [
                        new Part { Text = result.GetRawText()},
                    new Part { Text = resultCall.GetRawText() }
                    ]
                });

                request.Contents.Add(new Content
                {
                    Role = "user",
                    Parts =
                    [
                        new Part { Text = text}
                    ]
                });

                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(request);

                Console.WriteLine(e);
                return true;
            }
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
}
