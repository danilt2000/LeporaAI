using HepaticaAI.Brain.Models;
using HepaticaAI.Core;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using HepaticaAI.Core.Models.Messages;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HepaticaAI.Brain.Services;

public class PythonWebSocketDiscordSpeechRecognition : ISpeechRecognition
{
    private ClientWebSocket _ws;
    private readonly Uri _serverUri = new("ws://localhost:8765");
    private readonly IMemory _memory;
    private readonly DiscordService _discordService;

    private readonly CancellationTokenSource _cts = new();
    private readonly List<MessageEntry> _voiceChatMessageQueue = new();

    private DateTime _flagLastActivated = DateTime.MinValue;
    private bool _flagIsActive;

    public PythonWebSocketDiscordSpeechRecognition(IMemory memory, DiscordService discordService)
    {
        _memory = memory;
        _discordService = discordService;
        _ws = new ClientWebSocket();
    }

    public void Start()
    {
        Debug.WriteLine("🚀 Starting WebSocket in background...");
        Task.Run(() => RunWebSocket(_cts.Token));
    }

    private async Task RunWebSocket(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await ConnectWebSocket();
                await ReceiveMessages(token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ WebSocket error: {ex.Message}");
                await Task.Delay(2000, token);
            }
        }
    }

    private async Task ConnectWebSocket()
    {
        if (_ws.State == WebSocketState.Open)
            return;

        _ws.Dispose();
        _ws = new ClientWebSocket();

        Debug.WriteLine("🔄 Connecting to WebSocket...");
        await _ws.ConnectAsync(_serverUri, CancellationToken.None);
        Debug.WriteLine("✅ WebSocket connected");
    }

    private async Task ReceiveMessages(CancellationToken token)
    {
        byte[] receiveBuffer = new byte[1024];

        while (_ws.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.WriteLine("🔴 Server closed connection");
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                Debug.WriteLine($"📥 Received: {receivedMessage}");

                var data = JsonSerializer.Deserialize<SpeechResultDiscordWebSocket>(receivedMessage);

                if (data?.type == "Intermediate speech")
                {
                    Debug.WriteLine($"📥 Intermediate: {data.user}, {data.result}");
                    TouchFlag();
                }
                else if (!string.IsNullOrEmpty(data?.result))
                {
                    Debug.WriteLine($"📥 Final: {data.user}, {data.result}");

                    var username = await _discordService.GetUsernameByIdAsync((ulong)data.user);

                    _voiceChatMessageQueue.Add(new MessageEntry(username!, data.result));

                    if (!IsFlagRecentlyActivated())
                    {
                        _memory.AddEntitiesToProcessInQueue(_voiceChatMessageQueue);
                        _voiceChatMessageQueue.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Receive error: {ex.Message}");
                break;
            }
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_ws.State != WebSocketState.Open)
        {
            Debug.WriteLine("⚠️ WebSocket is not connected. Unable to send message.");
            return;
        }

        try
        {
            byte[] encodedMessage = Encoding.UTF8.GetBytes(message);
            await _ws.SendAsync(new ArraySegment<byte>(encodedMessage), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.WriteLine($"📤 Sent: {message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Send error: {ex.Message}");
        }
    }
}
