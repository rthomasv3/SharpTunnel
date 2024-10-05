using Microsoft.AspNetCore.SignalR;
using SharpTunnel.Shared.Models;
using SharpTunnel.Web.Services;
using System;
using System.Threading.Tasks;

namespace SharpTunnel.Web.Hubs;

public class TunnelHub : Hub
{
    private readonly TunnelService _tunnelService;

    public TunnelHub(TunnelService tunnelService)
    {
        _tunnelService = tunnelService;
    }

    public void GetResponse(TunnelMessage message)
    {
        _tunnelService.ReceiveMessage(message);
    }

    public override Task OnConnectedAsync()
    {
        _tunnelService.AddClient(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _tunnelService.RemoveClient(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
