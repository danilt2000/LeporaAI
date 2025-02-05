namespace HepaticaAI.Core.Interfaces.Translations
{
    public interface ITranslation
    {
        Task<string> TranslateEngtoRu(string words);
        Task<string> TranslateRutoEng(string words);
    }
}
