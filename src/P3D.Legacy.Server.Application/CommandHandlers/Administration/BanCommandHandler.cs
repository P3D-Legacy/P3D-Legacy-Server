using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Infrastructure.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration
{
    public class BanCommandHandler : IRequestHandler<BanCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly BanRepository _banRepository;

        public BanCommandHandler(ILogger<BanCommandHandler> logger, IMediator mediator, BanRepository banRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

        public async Task<CommandResult> Handle(BanCommand request, CancellationToken ct)
        {
            var (id, name, ip, reason, expiration) = request;

            var result = await _banRepository.Upsert(id, name, ip, reason, expiration, ct);
            return new CommandResult(result);
        }
    }
}