using HepaticaAI.Core.Models.Messages;

namespace HepaticaAI.Core.Interfaces.Memory
{
        public interface IMemory
        {
                void AddEntry(string role, string message);

                void AddUserRoleToStopSequenceIfMissing(string role);
                List<string> GetStopSequence();

                void DeleteLastMessage();

                void AddEntryToProcessInQueue(string role, string message);

                string GetFormattedPrompt();

                string GetFormattedPromptWithoutMemoryForgeting();

                void Clear();

                bool HasMessagesToProcess();

                bool IsNotCurrentlyProcessingMessage();

                void StartProcessing();

                void StopProcessing();

                MessageEntry GetMessageToProcess();
        }
}
