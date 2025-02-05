using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HepaticaAI.Brain.Services
{
    internal class DiscordSpeechRecognition(IConfiguration configuration) : ISpeechRecognition
    {
        private readonly ConcurrentDictionary<ulong, MemoryStream> _userAudioBuffers = new();
        private readonly ConcurrentDictionary<ulong, object> _userLocks = new();
        private DiscordClient? _discord;
        private WhisperSpeechRecognition? _whisperService;

        public async Task Initialize()
        {
            _whisperService = new WhisperSpeechRecognition();
            await _whisperService.InitializeModelAsync("whisper-base.en-q5_1.bin");

            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = configuration["DiscordBotToken"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            var voiceConfig = new VoiceNextConfiguration
            {
                AudioFormat = AudioFormat.Default,
                EnableIncoming = true
            };

            _discord.UseVoiceNext(voiceConfig);

            _discord.MessageCreated += async (client, e) =>
            {
                if (e.Message.Content.StartsWith("!join"))
                {
                    await HandleJoinCommand(client, e);
                }
            };

            _ = Task.Run(PeriodicWhisperRecognitionLoop);
            await _discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task HandleJoinCommand(DiscordClient client, MessageCreateEventArgs e)
        {
            try
            {
                var guild = await client.GetGuildAsync(1175432324603719774);
                var voiceChannel = guild?.Channels.Values.FirstOrDefault(c => c.Id == 1214942854213009443);

                if (voiceChannel != null && voiceChannel.Type == ChannelType.Voice)
                {
                    var vnc = await voiceChannel.ConnectAsync();
                    vnc.VoiceReceived += VoiceReceivedHandler;
                    Debug.WriteLine($"Connected to voice channel: {voiceChannel.Name}");
                }
                else
                {
                    Debug.WriteLine("Voice channel not found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Join error: {ex}");
            }
        }

        private Task VoiceReceivedHandler(VoiceNextConnection sender, VoiceReceiveEventArgs e)
        {
            if (e.User == null) return Task.CompletedTask;

            var userId = e.User.Id;
            var userLock = _userLocks.GetOrAdd(userId, _ => new object());

            lock (userLock)
            {
                var buffer = _userAudioBuffers.GetOrAdd(userId, _ => new MemoryStream());
                buffer.Write(e.PcmData.ToArray(), 0, e.PcmData.Length);
            }

            return Task.CompletedTask;
        }

        private async Task PeriodicWhisperRecognitionLoop()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                foreach (var userId in _userAudioBuffers.Keys.ToList())
                {
                    if (!_userLocks.TryGetValue(userId, out var userLock)) continue;

                    lock (userLock)
                    {
                        if (!_userAudioBuffers.TryGetValue(userId, out var stream) || stream.Length == 0)
                            continue;

                        var audioBytes = stream.ToArray();
                        stream.SetLength(0);

                        _ = ProcessAudioAsync(userId, audioBytes);
                    }
                }
            }
        }

        private async Task ProcessAudioAsync(ulong userId, byte[] audioBytes)
        {
            try
            {
                var text = await _whisperService!.RecognizeSpeechUsingWhisper(
                    audioBytes,
                    48000  // Was incorrectly using 16000 here before
                );
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Debug.WriteLine($"[User {userId}]: {text}");
                    // Add your logic to handle recognized text here
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Processing error: {ex}");
            }
        }

        public async void Dispose()
        {
            if (_discord != null)
            {
                await _discord.DisconnectAsync();
                _discord.Dispose();
            }
            _whisperService?.Dispose();
        }
    }
}