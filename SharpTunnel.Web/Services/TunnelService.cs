using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharpTunnel.Shared.Models;
using SharpTunnel.Web.Models;

namespace SharpTunnel.Web.Services;

public class TunnelService
{
    #region Fields

    private readonly ConcurrentQueue<TunnelMessage> _messages = new();
    private WebSocket _tunnelSocket;

    #endregion

    #region Events

    public event EventHandler<WebSocketTunnelMessage> MessageReceived;

    #endregion

    #region Constructor(s)

    public TunnelService()
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
