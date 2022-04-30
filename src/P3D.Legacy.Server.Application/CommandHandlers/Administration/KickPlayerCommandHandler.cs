using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class KickPlayerCommandHandler : ICommandHandler<KickPlayerCommand>
    {
        private readonly ILogger _logger;

        public KickPlayerCommandHandler(ILogger<KickPlayerCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CommandResult> Handle(KickPlayerCommand request, CancellationToken ct)
        {
            var (player, reason) = request;

            await player.KickAsync(reason, ct);

            return new CommandResult(true);
        }
    }
}