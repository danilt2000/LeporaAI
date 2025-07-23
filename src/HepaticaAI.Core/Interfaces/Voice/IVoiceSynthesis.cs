namespace HepaticaAI.Core.Interfaces.Voice
{
    public interface IVoiceSynthesis
    {
        void Speak(string text);

        string GenerateSpeakAudioAndGetFilePath(string text);

        TimeSpan GetAudioDuration(string filePath);
    }
}
