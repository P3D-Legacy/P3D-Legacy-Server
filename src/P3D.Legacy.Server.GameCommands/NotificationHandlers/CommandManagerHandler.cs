using MediatR;

using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.GameCommands.CommandManagers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.NotificationHandlers
{
    internal sealed class CommandManagerHandler :
        INotificationHandler<PlayerSentCommandNotification>
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IMediator _mediator;
        private readonly NotificationPublisher _notificationPublisher;
        private readonly IReadOnlyList<CommandManager> _commandManagers;

        public CommandManagerHandler(ILogger<CommandManagerHandler> logger, TracerProvider traceProvider, IMediator mediator, NotificationPublisher notificationPublisher, IEnumerable<CommandManager> commandManagers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
            _commandManagers = commandManagers.ToList();
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.GameCommands");
        }

        private async Task SendMessageAsync(IPlayer player, string message, CancellationToken ct)
        {
            await _notificationPublisher.Publish(new MessageToPlayerNotification(IPlayer.Server, player, message), ct);
        }

        private async Task HandleCommandAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            using var span = _tracer.StartActiveSpan("CommandManagerHandler HandleCommand");

            var command = FindByName(alias) ?? FindByAlias(alias);
            if (command is null)
            {
                await SendMessageAsync(player, $@"Invalid command ""{alias}"".", ct);
                return;
            }

            if (command.LogCommand && (player.Permissions & PermissionFlags.UnVerified) == 0)
                _logger.LogInformation("{PlayerName}: /{CommandAlias} {CommandArgs}", player.Name, alias, string.Join(" ", arguments));

            if (command.Permissions == PermissionFlags.None)
            {
                await SendMessageAsync(player, "Command is not found!", ct);
                //await SendMessageAsync(player, @"Command is disabled!", ct);
                return;
            }

            if ((player.Permissions & command.Permissions) == PermissionFlags.None)
            {
                await SendMessageAsync(player, "Command is not found!", ct);
                //await SendMessageAsync(player, @"You have not the permission to use this command!", ct);
                return;
            }

            using var commandSpan = _tracer.StartActiveSpan($"{command.GetType().Name} Handle");
            await command.HandleAsync(player, alias, arguments, ct);
        }

        public CommandManager? FindByName(string name) => _commandManagers
            .Where(x => x.Permissions != PermissionFlags.None)
            .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        public CommandManager? FindByAlias(string alias) => _commandManagers
            .Where(x => x.Permissions != PermissionFlags.None)
            .FirstOrDefault(x => x.Aliases.Contains(alias, StringComparer.OrdinalIgnoreCase));

        public IReadOnlyList<CommandManager> GetCommands() => _commandManagers;


        public async Task Handle(PlayerSentCommandNotification notification, CancellationToken ct)
        {
            using var span = _tracer.StartActiveSpan("CommandManagerHandler Handle");

            var (player, message) = notification;

            var commandWithoutSlash = message.TrimStart('/');

            var messageArray = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)", RegexOptions.Compiled)
                .Split(commandWithoutSlash)
                .Select(str => str.TrimStart('"').TrimEnd('"'))
                .ToArray();
            //var messageArray = commandWithoutSlash.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (messageArray.Length == 0)
            {
                await SendMessageAsync(player, "Invalid command!", ct);
                return;
            }

            var alias = messageArray[0];
            var trimmedMessageArray = messageArray.Skip(1).ToArray();
            await HandleCommandAsync(player, alias, trimmedMessageArray, ct);
        }
    }
}