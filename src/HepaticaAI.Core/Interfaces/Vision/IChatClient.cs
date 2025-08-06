namespace HepaticaAI.Core.Interfaces.Vision
{
    public interface IChatClient
    {
        Task Connect();

        Task<string?> ScheduleLivestreamAsync(DateTime scheduledStartTimeUtc, string title, string description);
    }
}
