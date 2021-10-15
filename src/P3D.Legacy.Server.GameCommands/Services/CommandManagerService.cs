using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Notifications;
using P3D.Legacy.Server.GameCommands.CommandManagers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Services
{
    public sealed class CommandManagerService
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IReadOnlyList<CommandManager> _commandManagers;

        public CommandManagerService(ILogger<CommandManagerService> logger, IMediator mediator, IEnumerable<CommandManager> commandManagers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _commandManagers = commandManagers.ToList() ?? throw new ArgumentNullException(nameof(commandManagers));
        }

        private async Task SendMessageAsync(IPlayer player, string message, CancellationToken ct)
        {
            await _mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, player, message), ct);
        }

        /// <summary>
        /// Return <see langword="false"/> if <see cref="CommandManager"/> not found.
        /// </summary>
        public async Task<bool> ExecuteClientCommandAsync(IPlayer client, string message, CancellationToken ct)
        {
            var commandWithoutSlash = message.TrimStart('/');

            var messageArray = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)").Split(commandWithoutSlash).Select(str => str.TrimStart('"').TrimEnd('"')).ToArray();
            //var messageArray = commandWithoutSlash.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (messageArray.Length == 0)
                return false; // command not found

            var alias = messageArray[0];
            var trimmedMessageArray = messageArray.Skip(1).ToArray();

            if (!_commandManagers.Any(c => c.Name == alias || c.Aliases.Any(a => a == alias)))
                return false; // command not found

            await HandleCommandAsync(client, alias, trimmedMessageArray, ct);

            return true;
        }

        private async Task HandleCommandAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            var command = FindByName(alias) ?? FindByAlias(alias);
            if (command == null)
            {
                await SendMessageAsync(client, $@"Invalid command ""{alias}"".", ct);
                return;
            }

            if (command.LogCommand && (client.Permissions & PermissionFlags.UnVerified) == 0)
                _logger.LogInformation("{PlayerName}: /{CommandAlias} {CommandArgs}", client.Name, alias, string.Join(" ", arguments));

            if (command.Permissions == PermissionFlags.None)
            {
                await SendMessageAsync(client, @"Command is disabled!", ct);
                return;
            }

            if ((client.Permissions & command.Permissions) == PermissionFlags.None)
            {
                await SendMessageAsync(client, @"You have not the permission to use this command!", ct);
                return;
            }

            await command.HandleAsync(client, alias, arguments, ct);
        }

        public CommandManager? FindByName(string name) => _commandManagers.FirstOrDefault(command => command.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        public CommandManager? FindByAlias(string alias) => _commandManagers.FirstOrDefault(command => command.Aliases.Contains(alias, StringComparer.OrdinalIgnoreCase));

        public IReadOnlyList<CommandManager> GetCommands() => _commandManagers;
    }
}