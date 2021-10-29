using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration
{
    internal class KickPlayerCommandHandler : IRequestHandler<KickPlayerCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public KickPlayerCommandHandler(ILogger<KickPlayerCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<CommandResult> Handle(KickPlayerCommand request, CancellationToken ct)
        {
            var (player, reason) = request;

            await player.KickAsync(reason, ct);

            return new CommandResult(true);
        }
    }
}