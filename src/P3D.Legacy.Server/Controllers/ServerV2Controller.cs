using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Controllers
{
    [ApiController]
    [Route("api/v2/server")]
    public class ServerV2Controller : ControllerBase
    {
        public sealed record StatusResponseV2Player(string Name, ulong GameJoltId);
        public sealed record StatusResponseV2(IEnumerable<StatusResponseV2Player> Players);

        private readonly ILogger _logger;
        private readonly PlayerHandlerService _playerHandlerService;

        public ServerV2Controller(ILogger<ServerV2Controller> logger, PlayerHandlerService playerHandlerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerHandlerService = playerHandlerService ?? throw new ArgumentNullException(nameof(playerHandlerService));
        }

        [HttpGet("status")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StatusResponseV2), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public ActionResult GetStatus() => Ok(new StatusResponseV2(_playerHandlerService.Players.Select(x => new StatusResponseV2Player(x.Name, x.GameJoltId))));
    }
}