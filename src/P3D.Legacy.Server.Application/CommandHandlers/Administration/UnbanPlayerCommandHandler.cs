using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class UnbanPlayerCommandHandler : ICommandHandler<UnbanPlayerCommand>
    {
        private readonly ILogger _logger;
        private readonly IBanManager _banManager;

        public UnbanPlayerCommandHandler(ILogger<UnbanPlayerCommandHandler> logger, IBanManager banManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _banManager = banManager ?? throw new ArgumentNullException(nameof(banManager));
        }

        public async Task<CommandResult> Handle(UnbanPlayerCommand request, CancellationToken ct)
        {
            var result = await _banManager.UnbanAsync(request.Id, ct);

            return new CommandResult(result);
        }
    }
}