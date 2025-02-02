namespace HepaticaAI.Core.Interfaces.Translations
{
        public interface ITranslation
        {
                Task<string> Translate(string words);
        }
}
