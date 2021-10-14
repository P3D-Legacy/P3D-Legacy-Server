using MediatR;

using P3D.Legacy.Server.Abstractions;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.Chat
{
    public class SayCommandManager : CommandManager
    {
        public override string Name => "say";
        public override string Description => "Speak as the Server.";
        public override Permissions Permissions => Permissions.AdministratorOrHigher;

        public SayCommandManager(IMediator mediator) : base(mediator) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var message = arguments[0].TrimStart('"').TrimEnd('"');
                ModuleManager.SendServerMessage(message);
            }
            else if (arguments.Length > 1)
            {
                var message = string.Join(" ", arguments);
                ModuleManager.SendServerMessage(message);
            }
            else
                await SendMessage(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessage(client, $"Correct usage is /{alias} <Message>", ct);
        }
    }
}