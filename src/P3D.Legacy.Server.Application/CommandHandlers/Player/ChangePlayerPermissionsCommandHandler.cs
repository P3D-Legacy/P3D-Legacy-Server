using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class ChangePlayerPermissionsCommandHandler : IRequestHandler<ChangePlayerPermissionsCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public ChangePlayerPermissionsCommandHandler(ILogger<ChangePlayerPermissionsCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public Task<CommandResult> Handle(ChangePlayerPermissionsCommand request, CancellationToken ct)
        {
            return Task.FromResult(new CommandResult(false));
        }
    }
}