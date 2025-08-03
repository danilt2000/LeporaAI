using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.Vision;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace HepaticaAI.Vision.Services
{
    internal class YoutubeChatClient : IChatClient, IDisposable
    {
        //chat_downloader https://www.youtube.com/watch?v=ajgiAZqNjIY

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

        public async Task Connect()
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

                        _memory.AddEntryToProcessInQueue(author, message);
                        OnMessageReceived(author, message);
                    }

                    break;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("The channel is not currently live", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("❌ Channel not live yet. Retrying in 3 seconds...");
                        await Task.Delay(3000);
                        continue;
                    }

                    Console.WriteLine("❌ Unexpected error:");
                    Console.WriteLine(e);
                    throw;
                }
            }
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
