namespace HepaticaAI.Core.Interfaces.Tracking
{
        public interface IChatTracking
        {
                string GetRecentMessages(TimeSpan timeSpan);
        }
}
