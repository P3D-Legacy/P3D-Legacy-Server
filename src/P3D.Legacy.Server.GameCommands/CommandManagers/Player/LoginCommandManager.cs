using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player;

internal class LoginCommandManager : CommandManager
{
    public override string Name => "login";
    public override string Description => "Log in the Server.";
    public override IEnumerable<string> Aliases => new[] { "l" };
    public override PermissionTypes Permissions => PermissionTypes.UnVerified;

    public LoginCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
    {
        if (arguments.Length == 1)
            await EventDispatcher.DispatchAsync(new PlayerSentLoginEvent(player, arguments[0]), ct);
        else
            await SendMessageAsync(player, "Invalid arguments given.", ct);
    }

    public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
    {
        await SendMessageAsync(player, $"Correct usage is /{alias} <playername> <password>", ct);
    }
}