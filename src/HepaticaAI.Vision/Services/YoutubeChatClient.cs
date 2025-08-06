using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTube.v3;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.Vision;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace HepaticaAI.Vision.Services
{
    internal class YoutubeChatClient : IChatClient, IDisposable
    {
        private readonly IMemory _memory;
        private readonly string _videoId;
        private Process? _chatProcess;

        public event Action<string, string> OnMessageReceived = delegate { };
        public event Action<string> OnConnected = delegate { };

        public YoutubeChatClient(IConfiguration configuration, IMemory memory)
        {
            _memory = memory;
            _videoId = "ajgiAZqNjIY";
        }

        public async Task<string?> GetStreamId(string url = "https://www.youtube.com/@hepatica42/live")
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-J \"{url}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return null;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"yt-dlp failed: {error}");

            using var doc = JsonDocument.Parse(output);
            if (doc.RootElement.TryGetProperty("id", out var videoIdElement))
            {
                return videoIdElement.GetString();
            }

            return null;
        }

        public async Task Connect()//TODO FIX ID CHANGE 
        {
            while (true)
            {
                try
                {
                    var streamId = await GetStreamId();

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "chat_downloader",
                        Arguments = $"https://www.youtube.com/watch?v={streamId}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    _chatProcess = Process.Start(startInfo)!;
                    OnConnected("YouTube");

                    while (!_chatProcess.HasExited)
                    {
                        var line = await _chatProcess.StandardOutput.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("[INFO]"))
                            continue;

                        var parts = line.Split('|', 2);
                        if (parts.Length < 2) continue;

                        var authorMsg = parts[1].Split(':', 2);
                        if (authorMsg.Length < 2) continue;

                        var author = authorMsg[0].Trim();
                        var message = authorMsg[1].Trim();
                        Console.WriteLine($"{author}:{message}");

                        _memory.AddEntryToProcessInQueue(author, message);
                        OnMessageReceived(author, message);
                    }

                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    await Task.Delay(3000);
                    continue;
                }
            }
        }

        private async Task<UserCredential> GetUserCredentialAsync()
        {
            using var stream = new FileStream(
                "client_secret_680158819564-dp4pgtom08fu554nv1np9pm169r7b11s.apps.googleusercontent.com.json",
                FileMode.Open, FileAccess.Read);

            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { YouTubeService.Scope.Youtube },
                "user",
                CancellationToken.None,
                new FileDataStore("token_storage", true)
            );
        }

        public async Task<string?> ScheduleLivestreamAsync(DateTime scheduledStartTimeUtc, string title, string description)
        {
            var credential = await GetUserCredentialAsync();

            var youtubeService = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "HepaticaAI"
            });

            var broadcast = new LiveBroadcast
            {
                Kind = "youtube#liveBroadcast",
                Snippet = new LiveBroadcastSnippet
                {
                    Title = title,
                    Description = description,
                    ScheduledStartTimeDateTimeOffset = new DateTimeOffset(scheduledStartTimeUtc, TimeSpan.Zero)
                },
                Status = new LiveBroadcastStatus
                {
                    PrivacyStatus = "public",
                    SelfDeclaredMadeForKids = false
                },
                ContentDetails = new LiveBroadcastContentDetails
                {
                    EnableAutoStart = true,
                    EnableAutoStop = true,
                    LatencyPreference = "ultraLow"
                }
            };

            var insertRequest = youtubeService.LiveBroadcasts.Insert(broadcast, "snippet,status,contentDetails");
            var response = await insertRequest.ExecuteAsync();
            return response.Id;
        }

        public void SendMessage(string message) =>
            throw new NotSupportedException("Sending messages is not supported.");

        public void Dispose()
        {
            if (_chatProcess is { HasExited: false })
                _chatProcess.Kill(entireProcessTree: true);
            _chatProcess?.Dispose();
        }
    }
}
