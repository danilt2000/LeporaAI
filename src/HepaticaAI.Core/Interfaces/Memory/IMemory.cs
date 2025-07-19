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

        void AddVoiceEntryToProcessInQueue(string role, string message);

        string GetFormattedPrompt();

        string GetFormattedPromptWithoutMemoryForgeting();

        void Clear();

        bool HasMessagesToProcess();

        public bool HasVoiceMessagesToProcess();

        bool IsNotCurrentlyProcessingMessage();

        void StartProcessing();

        void StopProcessing();

        MessageEntry GetMessageToProcess();

        List<MessageEntry> GetVoiceMessagesToProcess();
    }
}
