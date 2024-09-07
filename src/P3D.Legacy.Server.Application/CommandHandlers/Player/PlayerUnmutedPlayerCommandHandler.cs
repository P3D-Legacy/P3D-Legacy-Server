using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.Infrastructure.Repositories.Mutes;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player;

internal sealed class PlayerUnmutedPlayerCommandHandler : ICommandHandler<PlayerUnmutedPlayerCommand>
{
    private readonly ILogger _logger;
    private readonly IMuteRepository _muteRepository;

    public PlayerUnmutedPlayerCommandHandler(ILogger<PlayerUnmutedPlayerCommandHandler> logger, IMuteRepository muteRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _muteRepository = muteRepository ?? throw new ArgumentNullException(nameof(muteRepository));
    }

    public async Task<CommandResult> HandleAsync(PlayerUnmutedPlayerCommand command, CancellationToken ct)
    {
        var (id, idToUnmute) = command;

        await _muteRepository.UnmuteAsync(id, idToUnmute, ct);
        return new CommandResult(true);
    }
}