using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Application.Queries.Options;
using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Options;

internal sealed class GetServerOptionsQueryHandler : IQueryHandler<GetServerOptionsQuery, ServerOptions>
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;

    public GetServerOptionsQueryHandler(ILogger<GetServerOptionsQueryHandler> logger, IOptionsMonitor<ServerOptions> optionsMonitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
    }

    public Task<ServerOptions> HandleAsync(GetServerOptionsQuery query, CancellationToken ct)
    {
        return Task.FromResult(_optionsMonitor.CurrentValue);
    }
}