namespace HepaticaAI.Core.Interfaces.SpeechRecognition
{
    public interface ISpeechRecognition
    {
        void Start();
        
        Task SendMessageAsync(string message);
    }
}
