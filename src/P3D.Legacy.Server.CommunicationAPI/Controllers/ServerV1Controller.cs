using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Queries.Players;

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
        public sealed record StatusResponseV1Player(string Name, string GameJoltId);
        public sealed record StatusResponseV1(IEnumerable<StatusResponseV1Player> Players);

        private readonly ILogger _logger;

        public ServerV1Controller(ILogger<ServerV1Controller> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("status")]
        [ProducesResponseType(typeof(StatusResponseV1), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetStatusAsync([FromServices] IPlayerQueries playerQueries, CancellationToken ct) =>
            Ok(new StatusResponseV1(await playerQueries.GetAllAsync(ct).Select(x => new StatusResponseV1Player(x.Name, x.GameJoltId.ToString())).ToArrayAsync(ct)));
    }
}