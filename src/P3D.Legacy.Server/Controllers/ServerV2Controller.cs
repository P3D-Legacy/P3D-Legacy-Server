using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Queries.Players;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Controllers
{
    [ApiController]
    [Route("api/v2/server")]
    public class ServerV2Controller : ControllerBase
    {
        public sealed record StatusResponseV2Player(string Name, ulong GameJoltId);
        public sealed record StatusResponseV2(IEnumerable<StatusResponseV2Player> Players);

        private readonly ILogger _logger;

        public ServerV2Controller(ILogger<ServerV2Controller> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("status")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StatusResponseV2), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetStatusAsync([FromServices] IPlayerQueries playerQueries, CancellationToken ct) =>
            Ok(new StatusResponseV2(await playerQueries.GetAllAsync(ct).Select(x => new StatusResponseV2Player(x.Name, x.GameJoltId)).ToArrayAsync(ct)));
    }
}