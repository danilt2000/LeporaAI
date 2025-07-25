using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace HepaticaAI.Web.Services
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public string AddSocket(WebSocket socket)
        {
            var id = Guid.NewGuid().ToString();
            _sockets.TryAdd(id, socket);
            return id;
        }

        public async Task RemoveSocket(string id)
        {
            if (_sockets.TryRemove(id, out var socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
            }
        }

        public async Task BroadcastAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var tasks = _sockets.Values
                .Where(s => s.State == WebSocketState.Open)
                .Select(socket => socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
            await Task.WhenAll(tasks);
        }
    }
}
