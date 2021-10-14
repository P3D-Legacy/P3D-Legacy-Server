using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.GameCommands.Commands;
using P3D.Legacy.Server.GameCommands.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandHandlers
{
    // Command sending a command is an antipattern, but we're not using a DDD architecture, so we're fine.
    public class RawGameCommandHandler : IRequestHandler<RawGameCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly CommandManagerService _commandManagerService;

        public RawGameCommandHandler(ILogger<RawGameCommandHandler> logger, IMediator mediator, CommandManagerService commandManagerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _commandManagerService = commandManagerService ?? throw new ArgumentNullException(nameof(commandManagerService));
        }

        public async Task<CommandResult> Handle(RawGameCommand request, CancellationToken ct)
        {
            var (player, command) = request;

            var result = await _commandManagerService.ExecuteClientCommandAsync(player, command, ct);
            return new CommandResult(result);
        }
    }
}