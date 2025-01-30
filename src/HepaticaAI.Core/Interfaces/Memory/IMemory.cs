namespace HepaticaAI.Core.Interfaces.Memory
{
        public interface IMemory
        {
                void AddEntry(string role, string message);

                string GetFormattedPrompt();

                void Clear();
        }
}
