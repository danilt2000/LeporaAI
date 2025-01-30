namespace HepaticaAI.Core.Interfaces.AI
{
        public interface ILLMClient : IDisposable
        {
                void Initialize();

                Task<string> GenerateAsync(string parametrs, string prompt);
        }
}
