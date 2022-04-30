using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Queries;
using P3D.Legacy.Server.Application.Queries.Player;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Controllers
{
    [ApiController]
    [Route("api/v1/server")]
    public class ServerV1Controller : ControllerBase
    {
        private sealed record StatusResponseV1Player(string Name, string GameJoltId);
        private sealed record StatusResponseV1(IEnumerable<StatusResponseV1Player> Players);

        private readonly ILogger _logger;

        public ServerV1Controller(ILogger<ServerV1Controller> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("status")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StatusResponseV1), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetStatusAsync([FromServices] IQueryDispatcher queryDispatcher, CancellationToken ct) =>
            Ok(new StatusResponseV1((await queryDispatcher.DispatchAsync(new GetPlayerViewModelsQuery(),  ct)).Select(x => new StatusResponseV1Player(x.Name, x.GameJoltId.ToString()))));
    }
}