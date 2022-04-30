using P3D.Legacy.Server.Abstractions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Chat
{
    [SuppressMessage("Performance", "CA1812")]
    internal class SayCommandManager : CommandManager
    {
        public override string Name => "say";
        public override string Description => "Speak as the Server.";
        public override PermissionTypes Permissions => PermissionTypes.AdministratorOrHigher;

        public SayCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var message = arguments[0].TrimStart('"').TrimEnd('"');
                await SendServerMessageAsync(message, ct);
            }
            else if (arguments.Length > 1)
            {
                var message = string.Join(" ", arguments);
                await SendServerMessageAsync(message, ct);
            }
            else
                await SendMessageAsync(player, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <message>", ct);
        }
    }
}