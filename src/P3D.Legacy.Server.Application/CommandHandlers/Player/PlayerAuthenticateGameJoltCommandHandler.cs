using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerAuthenticateGameJoltCommandHandler : IRequestHandler<PlayerAuthenticateGameJoltCommand, CommandResult>
    {
        private readonly ILogger _logger;
        public PlayerAuthenticateGameJoltCommandHandler(ILogger<PlayerAuthenticateGameJoltCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<CommandResult> Handle(PlayerAuthenticateGameJoltCommand request, CancellationToken ct)
        {
            return Task.FromResult(new CommandResult(true));
        }
    }
}