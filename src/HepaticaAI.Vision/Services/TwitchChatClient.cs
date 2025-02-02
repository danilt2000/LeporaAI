using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.Vision;
using Microsoft.Extensions.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace HepaticaAI.Vision.Services
{
        internal class TwitchChatClient : IChatClient
        {
                private readonly IMemory _memory;
                private readonly TwitchClient _client;

                public event Action<string, string> OnMessageReceived = null!;
                public event Action<string> OnConnected = null!;

                public TwitchChatClient(IConfiguration configuration, IMemory memory)
                {
                        _memory = memory;
                        var credentials = new ConnectionCredentials(configuration["TwitchUsername"], configuration["TwitchBotAccessToken"]);
                        _client = new TwitchClient();
                        _client.Initialize(credentials, configuration["TwitchChannelToMonitor"]);

                        _client.OnMessageReceived += HandleMessageReceived!;
                        _client.OnConnected += HandleConnected!;
                }

                public void Connect() => _client.Connect();

                public void SendMessage(string message) => _client.SendMessage(_client.JoinedChannels[0], message);

                private void HandleMessageReceived(object sender, OnMessageReceivedArgs e)
                {
                        _memory.AddEntryToProcessInQueue(e.ChatMessage.DisplayName, e.ChatMessage.Message);

                        OnMessageReceived?.Invoke(e.ChatMessage.DisplayName, e.ChatMessage.Message);
                }

                private void HandleConnected(object sender, OnConnectedArgs e)
                {
                        OnConnected?.Invoke(e.BotUsername);
                }
        }
}
