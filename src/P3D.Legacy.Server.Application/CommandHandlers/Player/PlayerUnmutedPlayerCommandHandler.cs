using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Infrastructure.Services.Mutes;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class PlayerUnmutedPlayerCommandHandler : ICommandHandler<PlayerUnmutedPlayerCommand>
    {
        private readonly ILogger _logger;
        private readonly IMuteManager _muteRepository;

        public PlayerUnmutedPlayerCommandHandler(ILogger<PlayerUnmutedPlayerCommandHandler> logger, IMuteManager muteRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _muteRepository = muteRepository ?? throw new ArgumentNullException(nameof(muteRepository));
        }

        public async Task<CommandResult> Handle(PlayerUnmutedPlayerCommand request, CancellationToken ct)
        {
            await _muteRepository.UnmuteAsync(request.Id, request.IdToUnmute, ct);
            return new CommandResult(true);
        }
    }
}