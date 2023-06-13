using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Repositories.Bans;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration
{
    internal sealed class BanPlayerCommandHandler : ICommandHandler<BanPlayerCommand>
    {
        private readonly ILogger _logger;
        private readonly IBanRepository _banRepository;

        public BanPlayerCommandHandler(ILogger<BanPlayerCommandHandler> logger, IBanRepository banRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

        public async Task<CommandResult> HandleAsync(BanPlayerCommand command, CancellationToken ct)
        {
            var (bannerId, id, ip, reasonId, reason, expiration) = command;

            var result = await _banRepository.BanAsync(new BanEntity(bannerId, id, ip, reasonId, reason, expiration), ct);
            return new CommandResult(result);
        }
    }
}