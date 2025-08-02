using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.Vision;
using Microsoft.Extensions.Configuration;

namespace HepaticaAI.Vision.Services;

internal class YoutubeChatClient : IChatClient
{
    private readonly IMemory _memory;
    private readonly YouTubeService _youtube;
    private readonly string _liveChatId;
    private readonly CancellationTokenSource _cts = new();

    public event Action<string, string> OnMessageReceived = null!;
    public event Action<string> OnConnected = null!;

    public YoutubeChatClient(IConfiguration configuration, IMemory memory)
    {
        _memory = memory;
        var apiKey = configuration["YoutubeApiKey"];
        var broadcastId = "nVJ7uTx3PJ0"; 

        _youtube = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = apiKey,
            ApplicationName = "YouTubeLiveChatReader"
        });

        var videoRequest = _youtube.Videos.List("liveStreamingDetails");
        videoRequest.Id = broadcastId;
        var videoResponse = videoRequest.Execute();

        _liveChatId = videoResponse.Items.FirstOrDefault()?.LiveStreamingDetails?.ActiveLiveChatId
                      ?? throw new InvalidOperationException("Live chat not found.");
    }

    public void Connect()
    {
        _ = Task.Run(ReadChatLoopAsync);
        OnConnected?.Invoke("YouTube");
    }

    private async Task ReadChatLoopAsync()
    {
        string? pageToken = null;

        while (!_cts.IsCancellationRequested)
        {
            var request = _youtube.LiveChatMessages.List(_liveChatId, "snippet,authorDetails");
            request.PageToken = pageToken;

            var response = await request.ExecuteAsync(_cts.Token);

            foreach (var msg in response.Items)
            {
                var user = msg.AuthorDetails.DisplayName;
                var text = msg.Snippet.DisplayMessage;

                _memory.AddEntryToProcessInQueue(user, text);
                OnMessageReceived?.Invoke(user, text);
            }

            pageToken = response.NextPageToken;
            await Task.Delay((int)response.PollingIntervalMillis!, _cts.Token);
        }
    }

    public void SendMessage(string message)
    {
        throw new NotSupportedException("Sending messages is not supported with API key.");
    }
}