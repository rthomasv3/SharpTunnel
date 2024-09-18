using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
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
        _tunnelSocket = await webSocketManager.AcceptWebSocketAsync();

        // start listener, which will invoke message received events...
        // or start listener, which adds received messages to a queue, and have a method to get those messages...
    }

    #endregion

    #region Private Methods



    #endregion
}
