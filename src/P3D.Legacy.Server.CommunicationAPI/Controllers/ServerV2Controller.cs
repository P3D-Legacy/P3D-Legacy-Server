using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.CQERS.Queries;
using P3D.Legacy.Server.Infrastructure.Repositories.Statistics;
using P3D.Legacy.Server.UI.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Controllers
{
    [ApiController]
    [Route("api/v2/server")]
    public class ServerV2Controller : ControllerBase
    {
        public record StatusRequestV2Query(int Page, int PageSize);
        public sealed record StatusResponseV2Player(string Name, ulong GameJoltId);
        public sealed record ServerStatisticsResponse
        {
            public TimeSpan Uptime { get; init; }
            public uint Online { get; init; }
        }
        public sealed record PlayerStatisticsResponse
        {
            public uint Unique { get; init; }
            public uint SentMessages { get; init; }
        }

        private readonly ILogger _logger;

        public ServerV2Controller(ILogger<ServerV2Controller> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("status")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<StatusResponseV2Player>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAllAsync([FromQuery] StatusRequestV2Query query, [FromServices] IQueryDispatcher queryDispatcher, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 100), 5);

            var (count, models) = await queryDispatcher.DispatchAsync(new GetPlayerViewModelsPaginatedQuery((page - 1) * pageSize, pageSize), ct);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = count,
                TotalPages = (int) Math.Ceiling((double) count / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<StatusResponseV2Player>
            {
                Items = models.Select(static x => new StatusResponseV2Player(x.Name, x.GameJoltId)),
                Metadata = metadata,
            });
        }

        [HttpGet("metadata")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public ActionResult GetMetadata()
        {
            static IEnumerable<string> Metadata()
            {
                var assembly = Assembly.GetEntryAssembly();
                var buildDateTime = assembly?.GetCustomAttributes<BuildDateTimeAttribute>().FirstOrDefault()?.DateTime ?? DateTime.MinValue;

                yield return $"Binary Build Date: {buildDateTime:O}";
                yield return $"Binary Name: {assembly?.GetName().Name}";
                yield return $"Binary Version: {assembly?.GetName().Version}";
                yield return $"Git Repository: {ThisAssembly.Git.RepositoryUrl}";
                yield return $"Git Branch: {ThisAssembly.Git.Branch}";
                yield return $"Git Commit: {ThisAssembly.Git.Commit}";
                yield return $"Git Sha: {ThisAssembly.Git.Sha}";
                yield return $"Git BaseTag: {ThisAssembly.Git.BaseTag}";
                yield return $"Git Tag: {ThisAssembly.Git.Tag}";
                yield return $"Git Commit Date: {ThisAssembly.Git.CommitDate}";
            }
            return Ok(Metadata());
        }

        [HttpGet("statistics/server")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ServerStatisticsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetServerStatisticsAsync([FromServices] IQueryDispatcher queryDispatcher, CancellationToken ct)
        {
            var process = Process.GetCurrentProcess();
            var (onlineCount, _) = await queryDispatcher.DispatchAsync(new GetPlayerViewModelsPaginatedQuery(0, 0), ct);

            return Ok(new ServerStatisticsResponse
            {
                Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
                Online = (uint) onlineCount,
            });
        }

        [HttpGet("statistics/player")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PlayerStatisticsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetPlayerStatisticsAsync([FromServices] IStatisticsRepository statisticsRepository, CancellationToken ct)
        {
            return Ok(new PlayerStatisticsResponse
            {
                Unique = (uint) await statisticsRepository.GetAllAsync("player_joined", ct).CountAsync(ct),
                SentMessages = (uint) await statisticsRepository.GetAllAsync("message_global", ct).SumAsync(static x => x.Count, ct),
            });
        }
    }
}