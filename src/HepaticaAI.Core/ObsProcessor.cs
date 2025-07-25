using OBSWebsocketDotNet;

namespace HepaticaAI.Core
{
    public sealed class ObsProcessor : IAsyncDisposable
    {
        private readonly OBSWebsocket _ws = new();
        private readonly TaskCompletionSource<bool> _connectedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ObsProcessor()
        {
            _ws.Connected += OnConnected;
        }

        private void OnConnected(object? sender, EventArgs e)
        {
            _connectedTcs.TrySetResult(true);
        }

        public async Task ConnectAsync(string url, string password)
        {
            _ws.ConnectAsync(url, password);
            
            await Task.Delay(TimeSpan.FromSeconds(2));

            await _connectedTcs.Task.ConfigureAwait(false);
        }

        public Task StartStreamAsync()
        {
            if (!_ws.IsConnected)
                throw new InvalidOperationException("WebSocket не подключён");

            _ws.StartStream();
            return Task.CompletedTask;
        }

        public Task StopStreamAsync()
        {
            if (!_ws.IsConnected)
                throw new InvalidOperationException("WebSocket не подключён");

            _ws.StopStream();
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            _ws.Disconnect();
            return default;
        }
    }
}
