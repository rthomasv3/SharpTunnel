using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using SharpTunnel.Shared.Models;
using SharpTunnel.Web.Hubs;
using SharpTunnel.Web.Models;

namespace SharpTunnel.Web.Services;

public class TunnelService
{
    #region Fields

    private readonly ConcurrentQueue<TunnelMessage> _messages = new();
    private readonly ConcurrentDictionary<string, TunnelMessage> _messageMap = new();
    private readonly HashSet<string> _connectedClients = new();

    private WebSocket _tunnelSocket;

    #endregion

    #region Events

    public event EventHandler<WebSocketTunnelMessage> MessageReceived;

    #endregion

    #region Constructor(s)

    public TunnelService(IHubContext<TunnelHub> hubContext)
    {

    }

    #endregion

    #region Public Methods

    public async Task AcceptTunnelWebSocket(WebSocketManager webSocketManager)
    {
        // start listener, which will invoke message received events...
        // or start listener, which adds received messages to a queue, and have a method to get those messages...

        _tunnelSocket = await webSocketManager.AcceptWebSocketAsync();
        await ProcessMessages();
    }

    public void AddClient(string connectionId)
    {
        _connectedClients.Add(connectionId);
    }

    public void RemoveClient(string connectionId)
    {
        _connectedClients.Remove(connectionId);
    }

    public async Task WaitForTunnelConnection()
    {
        CancellationTokenSource tokenSource = new();
        tokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        while (_connectedClients.Count == 0)
        {
            await Task.Delay(100, tokenSource.Token);
        }
    }

    public void ReceiveMessage(TunnelMessage tunnelMessage)
    {
        _messageMap.TryAdd(tunnelMessage.TraceIdentifier, tunnelMessage);
    }

    public async Task<TunnelMessage> WaitForResponse(string traceId)
    {
        TunnelMessage message = null;

        try
        {
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                if (_messageMap.TryGetValue(traceId, out message))
                {
                    break;
                }
                await Task.Delay(5, cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
        }

        return message;
    }

    #endregion

    #region Private Methods

    private async Task ProcessMessages()
    {
        // Basic Echo

        var buffer = new byte[1024 * 4];
        var receiveResult = await _tunnelSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await _tunnelSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await _tunnelSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await _tunnelSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    #endregion
}
