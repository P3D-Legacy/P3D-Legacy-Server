using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommandHandlers
{
    // Command sending a command is an antipattern, but we're not using a DDD architecture, so we're fine.
    public class RawTextCommandHandler : IRequestHandler<RawTextCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public RawTextCommandHandler(ILogger<RawTextCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<CommandResult> Handle(RawTextCommand request, CancellationToken ct)
        {
            return new CommandResult(true);

            //return await _mediator.Send(request.Player, ct);
        }
    }
}