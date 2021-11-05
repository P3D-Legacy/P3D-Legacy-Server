using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration
{
    internal class BanPlayerCommandHandler : IRequestHandler<BanPlayerCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IBanManager _banRepository;

        public BanPlayerCommandHandler(ILogger<BanPlayerCommandHandler> logger, IMediator mediator, IBanManager banRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

        public async Task<CommandResult> Handle(BanPlayerCommand request, CancellationToken ct)
        {
            var (bannerId, id, ip, reasonId, reason, expiration) = request;

            var result = await _banRepository.BanAsync(new BanEntity(bannerId, id, ip, reasonId, reason, expiration), ct);
            return new CommandResult(result);
        }
    }
}