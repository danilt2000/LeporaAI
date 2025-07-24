using GenerativeAI;
using GenerativeAI.Types;

namespace HepaticaAI.Brain.Services.Gemini
{
    public class GeminiApiService
    {
        private readonly IGenerativeModel _model;
        public GeminiApiService(IGenerativeModel model) => _model = model;

        public async Task<string> SummarizeAsync(string text)
        {
            var request = new GenerateContentRequest();

            request.SystemInstruction = new Content
            {
                Role = "system",
                Parts = new List<Part>
    {
        new Part
        {
            Text = "ОТВЕЧАЙ В ФОРМАТЕ ПРОСТО СООБЩЕНИЯ — НЕ ПИШИ СВОЙ НИК." +
                    "НЕ ЮЗАЙ ЭМОДЗИ" +
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

            request.Contents = new List<Content>
{
    new Content
    {
        Role = "user",
        Parts = new List<Part>
        {
            new Part { Text = text }
        }
    }
};

            request.GenerationConfig = new GenerationConfig
            {
                MaxOutputTokens = 3000
            };

            var result = await _model.GenerateContentAsync(request);
            return result.Text.Trim();
        }
    }
}
