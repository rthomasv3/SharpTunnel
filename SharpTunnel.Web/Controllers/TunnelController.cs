using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharpTunnel.Web.Services;

namespace SharpTunnel.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TunnelController : ControllerBase
{
    #region Fields

    private readonly TunnelService _tunnelService;

    #endregion

    #region Constructor

    public TunnelController(TunnelService tunnelService)
    {
        _tunnelService = tunnelService;
    }

    #endregion

    #region Public Methods

    [Route("start")]
    public async Task<IActionResult> Start()
    {
        IActionResult result = BadRequest();

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            await _tunnelService.AcceptTunnelWebSocket(HttpContext.WebSockets);
        }

        return result;
    }

    #endregion

    #region Private Methods

    #endregion
}
