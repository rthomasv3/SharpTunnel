using Microsoft.AspNetCore.Http;
using SharpTunnel.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SharpTunnel.Web.MIddleware;

public class SignalRAuthMiddleware
{
    #region Fields

    private readonly RequestDelegate _next;
    private readonly Config _config;

    #endregion

    #region Constructor

    public SignalRAuthMiddleware(RequestDelegate next, Config config)
    {
        _next = next;
        _config = config;
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

            if (token == _config.SignalRBearerToken)
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
