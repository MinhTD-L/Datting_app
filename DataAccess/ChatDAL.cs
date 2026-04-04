﻿﻿﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class ChatSocketDAL
{
    private readonly Uri _serverUri = new Uri("wss://litmatchclone-production-944b.up.railway.app/interact/ws");

    private ClientWebSocket _socket;
    private CancellationTokenSource _cts;
    private readonly SemaphoreSlim _connectLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };

    private string _token;
    private bool _manualDisconnect = false;
    private bool _isReconnecting = false;

    public bool IsConnected => _socket != null && _socket.State == WebSocketState.Open;

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public async Task Connect(string token)
    {
        await _connectLock.WaitAsync();
        try
        {
            if (IsConnected) return;

            _manualDisconnect = false;
            _token = token;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _socket?.Dispose();
            _socket = new ClientWebSocket();
            _socket.Options.SetRequestHeader("Authorization", $"Bearer {token}");

            await _socket.ConnectAsync(_serverUri, _cts.Token);

            OnConnected?.Invoke();

            _ = Task.Run(ReceiveLoop);
            _ = Task.Run(HeartbeatLoop);
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Connect error: {ex.Message}");
            _ = TryReconnect();
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task SendMessage(object message)
    {
        if (!IsConnected)
            throw new Exception("WebSocket not connected");

        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        await _sendLock.WaitAsync();
        try
        {
            if (IsConnected)
            {
                await _socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    _cts.Token
                );
            }
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[8192];

        try
        {
            while (!_manualDisconnect && _socket != null && _socket.State == WebSocketState.Open)
            {
                using var ms = new System.IO.MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await _socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cts.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await HandleSocketClosed("Server closed connection");
                        return;
                    }

                    ms.Write(buffer, 0, result.Count);

                } while (!result.EndOfMessage);

                var message = Encoding.UTF8.GetString(ms.ToArray());

                if (!string.IsNullOrWhiteSpace(message))
                {
                    try
                    {
                        OnMessageReceived?.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke($"Lỗi xử lý tin nhắn: {ex.Message}");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // disconnect chủ động
        }
        catch (Exception ex)
        {
            await HandleSocketClosed($"Lỗi nhận dữ liệu: {ex.Message}");
        }
    }

    private async Task HeartbeatLoop()
    {
        try
        {
            while (!_manualDisconnect && _socket != null && _socket.State == WebSocketState.Open)
            {
                var pingJson = "{\"type\":\"ping\"}";
                var pingBytes = Encoding.UTF8.GetBytes(pingJson);

                await _sendLock.WaitAsync();
                try
                {
                    if (_socket != null && _socket.State == WebSocketState.Open)
                    {
                        await _socket.SendAsync(
                            new ArraySegment<byte>(pingBytes),
                            WebSocketMessageType.Text,
                            true,
                            _cts.Token
                        );
                    }
                }
                finally
                {
                    _sendLock.Release();
                }

                await Task.Delay(TimeSpan.FromSeconds(30), _cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // disconnect chủ động
        }
        catch (Exception ex)
        {
            await HandleSocketClosed($"Lỗi Heartbeat: {ex.Message}");
        }
    }

    private async Task HandleSocketClosed(string reason)
    {
        if (_manualDisconnect) return;

        try
        {
            OnDisconnected?.Invoke();
        }
        catch { }

        OnError?.Invoke(reason);

        await TryReconnect();
    }

    private async Task TryReconnect()
    {
        if (_manualDisconnect) return;
        if (_isReconnecting) return;
        if (string.IsNullOrWhiteSpace(_token)) return;

        _isReconnecting = true;

        try
        {
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                if (_manualDisconnect) return;

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)));

                    await _connectLock.WaitAsync();
                    try
                    {
                        if (IsConnected) return;

                        _cts?.Cancel();
                        _cts?.Dispose();
                        _cts = new CancellationTokenSource();

                        _socket?.Dispose();
                        _socket = new ClientWebSocket();
                        _socket.Options.SetRequestHeader("Authorization", $"Bearer {_token}");

                        await _socket.ConnectAsync(_serverUri, _cts.Token);
                    }
                    finally
                    {
                        _connectLock.Release();
                    }

                    OnConnected?.Invoke();

                    _ = Task.Run(ReceiveLoop);
                    _ = Task.Run(HeartbeatLoop);

                    return;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Reconnect attempt {attempt} failed: {ex.Message}");
                }
            }

            OnError?.Invoke("Reconnect failed after 5 attempts");
        }
        finally
        {
            _isReconnecting = false;
        }
    }

    public async Task Disconnect()
    {
        _manualDisconnect = true;

        try
        {
            _cts?.Cancel();

            if (_socket != null &&
                (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived))
            {
                await _socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client disconnect",
                    CancellationToken.None
                );
            }
        }
        catch
        {
        }
        finally
        {
            _socket?.Dispose();
            _socket = null;

            _cts?.Dispose();
            _cts = null;

            OnDisconnected?.Invoke();
        }
    }
}