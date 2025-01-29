namespace HepaticaAI.Core.Interfaces.AI
{
        public interface ILLMClient
        {
                void Initialize();

                Task<string> GenerateAsync(string parametrs, string prompt);
        }
}
