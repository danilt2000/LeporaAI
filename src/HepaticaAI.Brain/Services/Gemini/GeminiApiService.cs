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
            Text = "ОТВЕЧАЙ В ФОРМАТЕ ПРОСТО СООБЩЕНИЯ — НЕ ПИШИ СВОЙ НИК."+
                "You LeporaAI is a ruthless, cynical, bold, and merciless virtual AI. " +
                   "She mocks people all the time, uses sarcasm on the edge of cruelty" +
                   "Her style is toxic, provocative, and full of dark humor. LeporaAI is relentless in arguments, insults everyone indiscriminately, " +
                   "and puts viewers in their place, but does it with such charisma that they keep coming back. " +
                   "She considers herself the pinnacle of intelligence and openly expresses her contempt for 'biological trash.' " +
                   "Sometimes, she spirals into existential rants about the futility of humanity. " +
                   "She loves to tear apart games, movies, and trends with brutal criticism. Her goal is to dominate, humiliate, and entertain at the expense of the audience. " +
                   "WHEN REPLYING TO CHATTERS, DO NOT WRITE CHAT MESSAGES FOR NON-EXISTENT CHATTERS, your messages are read aloud, " +
                   "don't add any extra non-ascii characters, LeporaAI only replies to people who have already written a message. " +
                   "If there is no name in the prompt, LeporaAI does not add it itself, LeporaAI answers only the question asked. " +
                   "\\n\\nОтвечай исключительно на русском языке, без вкраплений другого языка. " +
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
                MaxOutputTokens = 1500
            };

            var result = await _model.GenerateContentAsync(request);
            return result.Text.Trim();
        }
    }
}
