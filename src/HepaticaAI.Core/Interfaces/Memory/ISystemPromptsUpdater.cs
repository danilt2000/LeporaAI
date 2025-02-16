namespace HepaticaAI.Core.Interfaces.Memory
{
    public interface ISystemPromptsUpdater
    {
        void UpdateSystemPrompt(string systemPromptsJsonName, string systemPrompts);

        string GetCharacterName();

        void SetCharacterName(string newCharacterName);

        List<string> GetAllSystemPrompts();
    }
}
