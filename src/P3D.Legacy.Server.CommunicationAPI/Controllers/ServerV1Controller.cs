using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Queries;
using P3D.Legacy.Server.Domain.Queries.Player;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Controllers;

[ApiController]
[Route("api/v1/server")]
public partial class ServerV1Controller : ControllerBase
{
    [JsonSerializable(typeof(StatusResponseV1))]
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
    public partial class ServerV1JsonContext : JsonSerializerContext;

    public sealed record StatusResponseV1Player(string Name, string GameJoltId);
    public sealed record StatusResponseV1(IEnumerable<StatusResponseV1Player> Players);

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
        Ok(new StatusResponseV1((await queryDispatcher.DispatchAsync(new GetPlayerViewModelsQuery(), ct)).Select(static x => new StatusResponseV1Player(x.Name, x.GameJoltId.ToString()))));
}