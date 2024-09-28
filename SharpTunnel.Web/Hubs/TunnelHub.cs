using Microsoft.AspNetCore.SignalR;
using SharpTunnel.Shared.Models;
using SharpTunnel.Web.Services;
using System.Threading.Tasks;

namespace SharpTunnel.Web.Hubs;

public class TunnelHub : Hub
{
    private readonly TunnelService _tunnelService;

    public TunnelHub(TunnelService tunnelService)
    {
        _tunnelService = tunnelService;
    }

    public async Task SendMessage(TunnelMessage message)
    {
        _tunnelService.ReceiveMessage(message);
        //await Clients.All.SendAsync("ReceiveMessage", message);
        //await Clients.Groups("some group").SendAsync("Hello", message);
    }
}
