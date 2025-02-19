using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using HepaticaAI.Core.Interfaces.SpeechRecognition;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Brain.Models;
using HepaticaAI.Core;

namespace HepaticaAI.Brain.Services;

public class PythonWebSocketDiscordSpeechRecognition : ISpeechRecognition
{
    private ClientWebSocket _ws;
    private readonly Uri _serverUri = new Uri("ws://localhost:8765");
    private readonly IMemory _memory;
    private readonly MessageProcessorSelector _messageProcessorSelector;
    private readonly CancellationTokenSource _cts = new();

    public PythonWebSocketDiscordSpeechRecognition(IMemory memory, MessageProcessorSelector messageProcessorSelector)
    {
        _memory = memory;
        _messageProcessorSelector = messageProcessorSelector;
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
                    Debug.WriteLine($"📥 Received: {data.user}, {data.result}");

                    _messageProcessorSelector.SetFalseIsNotPlayingIntermediateSpeech();
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

    public void Stop()
    {
        _cts.Cancel();
        Debug.WriteLine("🛑 WebSocket stopped.");
    }
}
