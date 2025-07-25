using System.Net.WebSockets;
using System.Text;
using HepaticaAI.Web.Services;

namespace HepaticaAI.Web
{
    public class WebSocketHandler
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketConnectionManager _manager;

        public WebSocketHandler(RequestDelegate next, WebSocketConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/messages")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    var id = _manager.AddSocket(socket);

                    var buffer = new byte[1024 * 4];
                    while (socket.State == WebSocketState.Open)
                    {
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _manager.RemoveSocket(id);
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            await _manager.BroadcastAsync(message); 
                        }
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }

    }
}
