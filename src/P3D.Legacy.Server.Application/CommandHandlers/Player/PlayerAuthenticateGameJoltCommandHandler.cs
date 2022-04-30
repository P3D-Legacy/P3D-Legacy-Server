using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class PlayerAuthenticateGameJoltCommandHandler : ICommandHandler<PlayerAuthenticateGameJoltCommand>
    {
        private readonly ILogger _logger;
        public PlayerAuthenticateGameJoltCommandHandler(ILogger<PlayerAuthenticateGameJoltCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<CommandResult> Handle(PlayerAuthenticateGameJoltCommand request, CancellationToken ct)
        {
            var (player, _) = request;

            Debug.Assert(player.State == PlayerState.Authentication);

            return Task.FromResult(new CommandResult(true));
        }
    }
}