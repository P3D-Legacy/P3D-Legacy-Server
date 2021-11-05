﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Queries.Players;
using P3D.Legacy.Server.UI.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Controllers
{
    [ApiController]
    [Route("api/v2/server")]
    public class ServerV2Controller : ControllerBase
    {
        public record StatusRequestV2Query(int Page, int PageSize);
        private sealed record StatusResponseV2Player(string Name, ulong GameJoltId);
        private sealed record StatusResponseV2(IEnumerable<StatusResponseV2Player> Players);

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

        [HttpGet("status/paginated")]
        public async Task<ActionResult> GetAll([FromQuery] StatusRequestV2Query query, [FromServices] IPlayerQueries playerQueries, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 100), 5);

            var (count, models) = await playerQueries.GetAllAsync((page - 1) * pageSize, pageSize, ct);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = count,
                TotalPages = (int) Math.Ceiling((double) count / (double) pageSize),
            };

            return StatusCode((int) HttpStatusCode.OK, new PagingResponse<StatusResponseV2Player>
            {
                Items = models.Select(x => new StatusResponseV2Player(x.Name, x.GameJoltId)),
                Metadata = metadata
            });
        }
    }
}