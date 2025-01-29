namespace HepaticaAI.Core.Interfaces.Tracking
{
        internal interface IChatTracking
        {
                string GetRecentMessages(TimeSpan timeSpan);
        }
}
