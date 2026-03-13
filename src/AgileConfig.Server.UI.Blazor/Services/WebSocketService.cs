using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace AgileConfig.Server.UI.Blazor.Services;

public class WebSocketAction
{
    public string Module { get; set; } = "";
    public string Action { get; set; } = "";
    public string Data { get; set; } = "";
}

public class WebSocketService : IAsyncDisposable
{
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;
    private readonly IConfiguration _configuration;

    public event Action<WebSocketAction>? OnMessage;
    public event Action<string>? OnError;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public WebSocketService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConnectAsync(string appId, string secret, string env = "DEV")
    {
        try
        {
            _ws = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            var baseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5000";
            var wsUrl = baseUrl.Replace("http://", "ws://").Replace("https://", "wss://");
            var uri = new Uri($"{wsUrl}/ws?appid={appId}&env={env}");

            // Add Basic Auth header
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appId}:{secret}"));
            _ws.Options.SetRequestHeader("Authorization", $"Basic {credentials}");

            await _ws.ConnectAsync(uri, _cts.Token);
            OnConnected?.Invoke();

            _ = Task.Run(ReceiveLoop);
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Connection error: {ex.Message}");
        }
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[4096];
        try
        {
            while (_ws?.State == WebSocketState.Open && !_cts!.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cts.Token);
                    OnDisconnected?.Invoke();
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var action = JsonSerializer.Deserialize<WebSocketAction>(message);
                        if (action != null)
                        {
                            OnMessage?.Invoke(action);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke($"Parse error: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Receive error: {ex.Message}");
        }
    }

    public async Task SendPingAsync()
    {
        if (_ws?.State != WebSocketState.Open) return;

        try
        {
            var ping = new WebSocketAction
            {
                Module = "c",
                Action = "ping",
                Data = ""
            };
            var json = JsonSerializer.Serialize(ping);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text, true, _cts!.Token);
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Send error: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        if (_ws != null)
        {
            if (_ws.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Closing", CancellationToken.None);
            }
            _ws.Dispose();
        }
        _cts?.Dispose();
    }
}
