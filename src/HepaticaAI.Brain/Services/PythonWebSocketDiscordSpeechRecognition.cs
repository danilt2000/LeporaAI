using HepaticaAI.Brain.Models;
using HepaticaAI.Core;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HepaticaAI.Brain.Services;

public class PythonWebSocketDiscordSpeechRecognition : ISpeechRecognition
{
    private ClientWebSocket _ws;
    private readonly Uri _serverUri = new Uri("ws://localhost:8765");
    private readonly IMemory _memory;
    private readonly VoiceMessageProcessorSelector _voiceMessageProcessorSelector;
    private readonly CancellationTokenSource _cts = new();
    private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(200);
    private Timer _timer;

    public PythonWebSocketDiscordSpeechRecognition(IMemory memory, VoiceMessageProcessorSelector voiceMessageProcessorSelector)
    {
        _memory = memory;
        _voiceMessageProcessorSelector = voiceMessageProcessorSelector;
        _ws = new ClientWebSocket();
    }

    private async void TimerCallback(object? state)
    {
        await SpeakMessageIfFilePathExist(state);
    }

    private async Task SpeakMessageIfFilePathExist(object? state)
    {
        if (!string.IsNullOrEmpty(_voiceMessageProcessorSelector.CurrentSpeakAudioPath))
        {
            await SendMessageAsync(_voiceMessageProcessorSelector.CurrentSpeakAudioPath);

            _voiceMessageProcessorSelector.CurrentSpeakAudioPath = string.Empty;
        }
    }

    public void Start()
    {
        Debug.WriteLine("🚀 Starting WebSocket in background...");
        _timer = new Timer(TimerCallback, null, TimeSpan.Zero, _interval);
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

    private readonly Dictionary<long, (string Message, DateTime Timestamp)> _recentMessages = new();

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
                    Debug.WriteLine($"📥 Received: {data.user}, {data.result}");

                    if (IsDuplicateMessage(data.user, data.result!))
                    {
                        Debug.WriteLine($"⚠️ Duplicate message detected from {data.user}. Sending stop signal.");
                        await SendStopSignal();
                    }

                    _voiceMessageProcessorSelector.SetFalseIsNotPlayingIntermediateSpeech();
                }
                else if (!string.IsNullOrEmpty(data?.result))
                {
                    Debug.WriteLine($"📥 Received: {data.user}, {data.result}");

                    //Todo apply there function GetUsernameByIdAsync for discord username 

                    _memory.AddVoiceEntryToProcessInQueue(data.user.ToString(), data.result);
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

    private bool IsDuplicateMessage(long user, string message)
    {
        if (_recentMessages.TryGetValue(user, out var entry) && entry.Message == message)
        {
            if ((DateTime.UtcNow - entry.Timestamp).TotalSeconds <= InteruptinIntervalInSeconds)
            {
                return true;
            }
        }
        _recentMessages[user] = (message, DateTime.UtcNow);
        return false;
    }

    private static int InteruptinIntervalInSeconds => 1;

    private async Task SendStopSignal()
    {
        var stopMessage = "STOP";

        await SendMessageAsync(stopMessage);
    }

    public void Stop()
    {
        _cts.Cancel();
        Debug.WriteLine("🛑 WebSocket stopped.");
    }
}
