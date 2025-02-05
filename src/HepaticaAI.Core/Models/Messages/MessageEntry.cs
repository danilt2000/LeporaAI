namespace HepaticaAI.Core.Models.Messages
{
    public class MessageEntry(string role, string message)
    {
        public string Role { get; } = role;
        public string Message { get; set; } = message;
    }
}
