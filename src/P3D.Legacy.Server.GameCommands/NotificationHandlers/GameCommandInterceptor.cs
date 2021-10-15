using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Notifications;
using P3D.Legacy.Server.GameCommands.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.NotificationHandlers
{
    public class GameCommandInterceptor : INotificationHandler<PlayerSentGlobalMessageNotification>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public GameCommandInterceptor(ILogger<GameCommandInterceptor> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task Handle(PlayerSentGlobalMessageNotification request, CancellationToken ct)
        {
            if (request.Message.StartsWith("/"))
                await _mediator.Send(new RawGameCommand(request.Player, request.Message[1..]), ct);
        }
    }
}