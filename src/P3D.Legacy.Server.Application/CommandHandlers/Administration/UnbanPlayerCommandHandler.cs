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
    internal class UnbanPlayerCommandHandler : IRequestHandler<UnbanPlayerCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IBanRepository _banRepository;

        public UnbanPlayerCommandHandler(ILogger<UnbanPlayerCommandHandler> logger, IMediator mediator, IBanRepository banRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

        public async Task<CommandResult> Handle(UnbanPlayerCommand request, CancellationToken ct)
        {
            var result = await _banRepository.DeleteAsync(request.Id, ct);

            return new CommandResult(result);
        }
    }
}