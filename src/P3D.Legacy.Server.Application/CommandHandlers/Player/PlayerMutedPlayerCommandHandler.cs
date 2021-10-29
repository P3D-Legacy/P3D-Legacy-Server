using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerMutedPlayerCommandHandler : IRequestHandler<PlayerMutedPlayerCommand, CommandResult>
    {
        private readonly ILogger _logger;

        public PlayerMutedPlayerCommandHandler(ILogger<PlayerMutedPlayerCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<CommandResult> Handle(PlayerMutedPlayerCommand request, CancellationToken ct)
        {
            return Task.FromResult(new CommandResult(true));
        }
    }
}