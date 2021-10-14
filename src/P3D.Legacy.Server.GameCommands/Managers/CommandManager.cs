using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Notifications;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers
{
    public abstract class CommandManager
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual IEnumerable<string> Aliases { get; } = Array.Empty<string>();
        public virtual Permissions Permissions { get; } = Permissions.None;
        public virtual bool LogCommand { get; } = true;

        protected IMediator Mediator { get; }

        protected CommandManager(IMediator mediator)
        {
            Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected IPlayer GetClient(string name)
        {
            return null;
        }

        protected async Task SendMessage(IPlayer player, string message, CancellationToken ct)
        {
            await Mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, player, message), ct);
        }

        public virtual async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            await HelpAsync(client, alias, ct);
        }

        public virtual async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessage(client, $@"Command ""{alias}"" is not functional!", ct);
        }
    }
}