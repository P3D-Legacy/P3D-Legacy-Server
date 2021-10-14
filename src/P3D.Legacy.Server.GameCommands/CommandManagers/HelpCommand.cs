using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.GameCommands.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers
{
    public class HelpCommand : CommandManager
    {
        public override string Name => "help";
        public override string Description => "Command help menu.";
        public override IEnumerable<string> Aliases => new [] { "h" };
        public override PermissionFlags Permissions => PermissionFlags.UnVerifiedOrHigher;

        private readonly CommandManagerService _commandManagerService;

        public HelpCommand(CommandManagerService commandManagerService, IMediator mediator) : base(mediator)
        {
            _commandManagerService = commandManagerService ?? throw new ArgumentNullException(nameof(commandManagerService));
        }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length > 1)
            {
                await HelpAsync(client, alias, ct);
                return;
            }

            var helpAlias = arguments.Length == 1 ? arguments[0] : "1";

            if (_commandManagerService.FindByName(helpAlias) is { } foundByName)
            {
                await foundByName.HelpAsync(client, helpAlias, ct);
                return;
            }
            if (_commandManagerService.FindByAlias(helpAlias) is { } foundByAlias)
            {
                await foundByAlias.HelpAsync(client, helpAlias, ct);
                return;
            }

            if (int.TryParse(helpAlias, out var pageNumber))
            {
                await HelpPageAsync(client, pageNumber, ct);
                return;
            }
            await HelpAsync(client, alias, ct);
        }
        private async Task HelpPageAsync(IPlayer client, int page, CancellationToken ct)
        {
            const int perPage = 5;
            var commands = _commandManagerService.GetCommands().Where(command => (client.Permissions & command.Permissions) != PermissionFlags.None).ToList();
            var numPages = (int) Math.Floor((double) commands.Count / perPage);
            if ((commands.Count % perPage) > 0)
                numPages++;

            if (page < 1 || page > numPages)
                page = 1;

            var startingIndex = (page - 1) * perPage;
            await SendMessageAsync(client, $"--Help page {page} of {numPages}--", ct);
            for (var i = 0; i < perPage; i++)
            {
                var index = startingIndex + i;
                if (index > commands.Count - 1)
                    break;

                var command = commands[index];
                await SendMessageAsync(client, $"/{command.Name} - {command.Description}", ct);
            }
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessageAsync(client, $"Correct usage is /{alias} <page#/command> [command arguments]", ct);
        }
    }
}