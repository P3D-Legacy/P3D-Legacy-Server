using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.Infrastructure.Repositories.Mutes;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class PlayerMutedPlayerCommandHandler : ICommandHandler<PlayerMutedPlayerCommand>
    {
        private readonly ILogger _logger;
        private readonly IMuteRepository _muteRepository;

        public PlayerMutedPlayerCommandHandler(ILogger<PlayerMutedPlayerCommandHandler> logger, IMuteRepository muteRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _muteRepository = muteRepository ?? throw new ArgumentNullException(nameof(muteRepository));
        }

        public async Task<CommandResult> HandleAsync(PlayerMutedPlayerCommand command, CancellationToken ct)
        {
            var (id, idToMute) = command;

            await _muteRepository.MuteAsync(id, idToMute, ct);
            return new CommandResult(true);
        }
    }
}