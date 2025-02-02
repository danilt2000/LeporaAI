using DeepL;
using HepaticaAI.Core.Interfaces.Translations;

namespace HepaticaAI.Brain.Services
{
        internal class DeplTranslation : ITranslation
        {
                public async Task<string> Translate(string words)
                {
                        try
                        {
                                var translator = new Translator("be229324-e9f1-425e-aa35-c7a042ea5536:fx");

                                var translatedText = await translator.TranslateTextAsync(
                                        words,
                                        LanguageCode.English,
                                        LanguageCode.Russian);
                                return translatedText.Text;
                        }
                        catch (Exception e)
                        {
                                return string.Empty;
                        }
                }
        }
}
