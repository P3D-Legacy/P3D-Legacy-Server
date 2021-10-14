using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.CommandHandlers.Player;
using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Notifications;
using P3D.Legacy.Server.GameCommands.CommandHandlers;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands
{
    public class PingGameCommandHandler : IRequestHandler<PingGameCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public PingGameCommandHandler(ILogger<RawGameCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<CommandResult> Handle(PingGameCommand request, CancellationToken ct)
        {
            //await request.Player.SendMessageAsync(IPlayer.Server, "Pong!", ct);
            await _mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, request.Player, "Pong!"), ct);

            return new CommandResult(true);
        }
    }
}