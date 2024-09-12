using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading.Tasks;

namespace SharpTunnel.MIddleware;

public class TunnelMiddleware
{
    #region Fields

    private readonly RequestDelegate _next;

    #endregion

    #region Constructor

    public TunnelMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    #endregion

    #region Public Methods

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path != "")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            }
            else
            {
                await _next(context);
            }
        }
    }

    #endregion
}
