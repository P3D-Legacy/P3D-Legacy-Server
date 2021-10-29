using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player
{
    internal class LoginCommandManager : CommandManager
    {
        public override string Name => "login";
        public override string Description => "Log in the Server.";
        public override IEnumerable<string> Aliases => new[] { "l" };
        public override PermissionFlags Permissions => PermissionFlags.UnVerified;

        public LoginCommandManager(IMediator mediator, IPlayerContainerReader playerContainer) : base(mediator, playerContainer) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
                await Mediator.Publish(new PlayerSentLoginNotification(player, arguments[0] + "A!"), ct);
            else
                await SendMessageAsync(player, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <playername> <password>", ct);
        }
    }
}