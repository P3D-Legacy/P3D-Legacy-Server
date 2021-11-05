using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration
{
    internal class UnbanPlayerCommandHandler : IRequestHandler<UnbanPlayerCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IBanManager _banManager;

        public UnbanPlayerCommandHandler(ILogger<UnbanPlayerCommandHandler> logger, IMediator mediator, IBanManager banManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _banManager = banManager ?? throw new ArgumentNullException(nameof(banManager));
        }

        public async Task<CommandResult> Handle(UnbanPlayerCommand request, CancellationToken ct)
        {
            var result = await _banManager.UnbanAsync(request.Id, ct);

            return new CommandResult(result);
        }
    }
}