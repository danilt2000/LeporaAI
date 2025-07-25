using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace HepaticaAI.Core
{
    public class SocketViewerWebSocketBridge
    {
        private ClientWebSocket _ws;
        private readonly Uri _serverUri = new("wss://localhost:9999/messages");

        private readonly CancellationTokenSource _cts = new();

        public SocketViewerWebSocketBridge()
        {
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
}
