using GenerativeAI;

namespace HepaticaAI.Brain.Services.Gemini
{
    public class GeminiApiService
    {
        private readonly IGenerativeModel _model;
        public GeminiApiService(IGenerativeModel model) => _model = model;

        public async Task<string> SummarizeAsync(string text)
        {
            var result = await _model.GenerateContentAsync(text);

            return result.Text;
        }
    }
}
