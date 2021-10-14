using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.GameCommands.Commands;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandHandlers
{
    // Command sending a command is an antipattern, but we're not using a DDD architecture, so we're fine.
    public class RawGameCommandHandler : IRequestHandler<RawGameCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public RawGameCommandHandler(ILogger<RawGameCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<CommandResult> Handle(RawGameCommand request, CancellationToken ct)
        {
            var (player, command) = request;

            var messageArray = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)").Split(command).Select(str => str.TrimStart('"').TrimEnd('"')).ToArray();

            if (messageArray.Length == 0)
                return new CommandResult(false);

            //if (command.StartsWith("ping", StringComparison.OrdinalIgnoreCase))
            //    return await _mediator.Send(new PingGameCommand(player), ct);

            if (command.StartsWith("ban", StringComparison.OrdinalIgnoreCase))
            {
                //return await _mediator.Send(new BanCommand(player.GameJoltId, player.Name, null, "", DateTimeOffset.Now), ct);
            }

            if (command.StartsWith("unban", StringComparison.OrdinalIgnoreCase))
            {
                //return await _mediator.Send(new UnbanCommand(player), ct);
            }

            return new CommandResult(true);

            //return await _mediator.Send(request.Player, ct);
        }
    }
}