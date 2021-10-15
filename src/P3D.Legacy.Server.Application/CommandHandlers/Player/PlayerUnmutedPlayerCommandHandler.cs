using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    public class PlayerUnmutedPlayerCommandHandler : IRequestHandler<PlayerUnmutedPlayerCommand, CommandResult>
    {
        private readonly ILogger _logger;

        public PlayerUnmutedPlayerCommandHandler(ILogger<PlayerUnmutedPlayerCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CommandResult> Handle(PlayerUnmutedPlayerCommand request, CancellationToken ct)
        {
            return new CommandResult(true);
        }
    }
}