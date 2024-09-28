using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SharpTunnel.Controllers;

[ApiController]
public class CatchAllController : ControllerBase
{
    [Route("/{**catchAll}")]
    public IActionResult CatchAll(string catchAll)
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


        return Ok($"Catch All: [{HttpContext.Request.Method}] {HttpContext.Request.Path} ({HttpContext.WebSockets.IsWebSocketRequest})");
    }
}
