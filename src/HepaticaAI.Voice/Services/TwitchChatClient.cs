using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HepaticaAI.Core.Interfaces.Vision;
using Microsoft.Extensions.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace HepaticaAI.Voice.Services
{
        internal class TwitchChatClient : IChatClient
        {
                private readonly TwitchClient _client;

                public event Action<string, string> OnMessageReceived;
                public event Action<string> OnConnected;

                public TwitchChatClient(IConfiguration configuration /*string username, string accessToken, string channelName*/)
                {
                        var credentials = new ConnectionCredentials(username, accessToken);
                        _client = new TwitchClient();
                        _client.Initialize(credentials, channelName);

                        _client.OnMessageReceived += HandleMessageReceived!;
                        _client.OnConnected += HandleConnected!;
                }

                public void Connect() => _client.Connect();

                public void SendMessage(string message) => _client.SendMessage(_client.JoinedChannels[0], message);

                private void HandleMessageReceived(object sender, OnMessageReceivedArgs e)
                {
                        OnMessageReceived?.Invoke(e.ChatMessage.DisplayName, e.ChatMessage.Message);
                }

                private void HandleConnected(object sender, OnConnectedArgs e)
                {
                        OnConnected?.Invoke(e.BotUsername);
                }
        }
}
