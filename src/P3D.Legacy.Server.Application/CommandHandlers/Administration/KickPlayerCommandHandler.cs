using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.CQERS.Commands;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration;

internal sealed class KickPlayerCommandHandler : ICommandHandler<KickPlayerCommand>
{
    private readonly ILogger _logger;

    public KickPlayerCommandHandler(ILogger<KickPlayerCommandHandler> logger)
    {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

    public async Task<CommandResult> HandleAsync(KickPlayerCommand command, CancellationToken ct)
    {
            var (player, reason) = command;

            await player.KickAsync(reason, ct);

            return new CommandResult(true);
        }
}