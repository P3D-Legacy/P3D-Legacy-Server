using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Bans;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Bans
{
    public class KickCommandHandler : IRequestHandler<KickCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IPlayerContainerActions _container;

        public KickCommandHandler(ILogger<KickCommandHandler> logger, IMediator mediator, IPlayerContainerActions container)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public async Task<CommandResult> Handle(KickCommand request, CancellationToken ct)
        {
            var result = await _container.KickAsync(request.Player, ct);

            return new CommandResult(result);
        }
    }
}