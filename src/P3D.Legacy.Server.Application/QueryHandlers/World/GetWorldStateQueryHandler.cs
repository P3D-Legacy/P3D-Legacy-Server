using Microsoft.Extensions.Logging;

using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Application.Queries.World;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.World;

internal sealed class GetWorldStateQueryHandler : IQueryHandler<GetWorldStateQuery, WorldState>
{
    private readonly ILogger _logger;
    private readonly WorldService _world;

    public GetWorldStateQueryHandler(ILogger<GetWorldStateQueryHandler> logger, WorldService world)
    {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

    public Task<WorldState> HandleAsync(GetWorldStateQuery query, CancellationToken ct)
    {
            return Task.FromResult(_world.State);
        }
}