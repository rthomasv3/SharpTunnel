using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SharpTunnel.Web.MIddleware;

public class SignalRAuthMiddleware
{
    #region Fields

    private readonly RequestDelegate _next;

    #endregion

    #region Constructor

    public SignalRAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    #endregion

    #region Public Methods

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/tunnelhub")
        {
            string token = context.Request.Headers.Authorization
                .ToString()
                .Replace("Bearer", String.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (token == "5BAC8098-E84A-4DE6-A9B6-D7756CD53AE1")
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 401;
            }
        }
        else
        {
            await _next(context);
        }
    }

    #endregion
}
