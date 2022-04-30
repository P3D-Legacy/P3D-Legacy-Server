using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class BanPlayerCommandHandler : ICommandHandler<BanPlayerCommand>
    {
        private readonly ILogger _logger;
        private readonly IBanManager _banRepository;

        public BanPlayerCommandHandler(ILogger<BanPlayerCommandHandler> logger, IBanManager banRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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