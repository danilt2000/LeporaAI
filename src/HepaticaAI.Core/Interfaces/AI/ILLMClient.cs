namespace HepaticaAI.Core.Interfaces.AI
{
        public interface ILLMClient
        {
                Task<string> GenerateAsync(string parametrs, string prompt);
        }
}
