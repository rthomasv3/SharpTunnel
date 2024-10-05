using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using SharpTunnel.Shared.Models;
using SharpTunnel.Web.Hubs;
using SharpTunnel.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpTunnel.Controllers;

[ApiController]
public class CatchAllController : ControllerBase
{
    private readonly IHubContext<TunnelHub> _hubContext;
    private readonly TunnelService _tunnelService;
    private readonly HashSet<string> _removeHeaders = ["Connection", "Transfer-Encoding", "Keep-Alive", "Upgrade", "Proxy-Connection"];

    public CatchAllController(IHubContext<TunnelHub> hubContext, TunnelService tunnelService)
    {
        _hubContext = hubContext;
        _tunnelService = tunnelService;
    }

    [Route("/{**catchAll}")]
    public async Task CatchAll(string catchAll)
    {
        // Steps
        // 1. Translate request path based on routing settings
        // 2. Record: Path, Method, Headers
        // 3. Get or generate unique request id
        // 4. Send request info over web socket connection to tunnel client
        // 5. Wait for tunnel client to respond with server response information
        // 6. Build and return response

        // Websockets
        // 1. Get or generate unique request id
        // 2. Send the websocket request information to the tunnel
        // 3. Wait for the tunnel to respond with the end server response
        //    * The tunnel will make a websocket connection to the end server
        // 4. Establish a websocket connection from this proxy server to the client
        // 5. When the tunnel gets data over its websocket, it will forward to this proxy server, which will forward over its socket to the client

        // Might also be possible to open a VPN connection with network tunnel is running on via Wireguard or OpenVPN
        // and then use something like YARP to proxy all connections because you'll be able to see the network
        // https://microsoft.github.io/reverse-proxy/articles/getting-started.html

        TunnelMessage tunnelMessage = await BuildRequest();

        //CancellationTokenSource timoutToken = new();
        //TunnelMessage response = await _hubContext.Clients.Client(_tunnelService.ConnectionId)
        //    .InvokeAsync<TunnelMessage>("ReceiveMessage", tunnelMessage, timoutToken.Token);

        await _tunnelService.WaitForTunnelConnection();

        await _hubContext.Clients.All.SendAsync("ReceiveMessage", tunnelMessage);
        TunnelMessage response = await _tunnelService.WaitForResponse(HttpContext.TraceIdentifier);

        if (response != null)
        {
            BuildResponse(response.Response);
        }
        else
        {
            HttpContext.Response.StatusCode = 404;
        }

        //return Ok($"Catch All: [{HttpContext.Request.Method}] {HttpContext.Request.Path} ({HttpContext.WebSockets.IsWebSocketRequest})");
    }

    private async Task<TunnelMessage> BuildRequest()
    {
        using MemoryStream ms = new();
        await HttpContext.Request.Body.CopyToAsync(ms);
        byte[] body = ms.ToArray();

        Dictionary<string, string> form = null;
        if (HttpContext.Request.HasFormContentType)
        {
            form = HttpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
        }

        WebRequest webRequest = new()
        {
            Body = body,
            ContentLength = HttpContext.Request.ContentLength,
            ContentType = HttpContext.Request.ContentType,
            Cookies = HttpContext.Request.Cookies.ToDictionary(x => x.Key, x => x.Value),
            Headers = HttpContext.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Host = HttpContext.Request.Host.ToString(),
            IsWebSocketRequest = HttpContext.WebSockets.IsWebSocketRequest,
            Method = HttpContext.Request.Method,
            Path = HttpContext.Request.Path,
            Query = HttpContext.Request.QueryString.ToString(),
            Scheme = HttpContext.Request.Scheme,
            Form = form,
        };
        TunnelMessage tunnelMessage = new()
        {
            MessageType = Shared.Enums.TunnelMessageType.WebRequest,
            Name = "This is a POC",
            Request = webRequest,
            TraceIdentifier = HttpContext.TraceIdentifier,
        };

        return tunnelMessage;
    }

    private void BuildResponse(WebResponse response)
    {
        HttpContext.Response.StatusCode = response.StatusCode;
        HttpContext.Response.ContentType = response.ContentType;

        foreach (var header in response.Headers)
        {
            if (!_removeHeaders.Contains(header.Key))
            {
                HttpContext.Response.Headers.Append(header.Key, new StringValues(header.Value.ToArray()));
            }
        }

        // Set-Cookie header might be enough...
        //foreach (var cookie in response.Cookies)
        //{
        //    HttpContext.Response.Cookies.Append(cookie.Key, cookie.Value);
        //}

        if (response.Path != HttpContext.Request.Path)
        {
            string newPath = HttpContext.Request.Scheme + "://" +
                             HttpContext.Request.Host.ToString() +
                             response.Path.Substring(response.Path.IndexOf('/')) +
                             response.Query;
            HttpContext.Response.Headers.Remove("location");
            HttpContext.Response.Headers.Location = newPath;
            HttpContext.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
        }

        if (response.Body?.Length > 0 == true)
        {
            HttpContext.Response.ContentLength = response.ContentLength;
            HttpContext.Response.BodyWriter.WriteAsync(response.Body);
        }
        else
        {
            HttpContext.Response.BodyWriter.Complete();
        }
    }
}
