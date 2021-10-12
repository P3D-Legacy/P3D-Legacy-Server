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
    [Route("api/v1/server")]
    public class ServerV1Controller : ControllerBase
    {
        public sealed record StatusResponseV1Player(string Name, string GameJoltId);
        public sealed record StatusResponseV1(IEnumerable<StatusResponseV1Player> Players);

        private readonly ILogger _logger;
        private readonly PlayerHandlerService _playerHandlerService;

        public ServerV1Controller(ILogger<ServerV1Controller> logger, PlayerHandlerService playerHandlerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerHandlerService = playerHandlerService ?? throw new ArgumentNullException(nameof(playerHandlerService));
        }

        [HttpGet("status")]
        [ProducesResponseType(typeof(StatusResponseV1), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public ActionResult GetStatus() => Ok(new StatusResponseV1(_playerHandlerService.Players.Select(x => new StatusResponseV1Player(x.Name, x.GameJoltId.ToString()))));
    }
}