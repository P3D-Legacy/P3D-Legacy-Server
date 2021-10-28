using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers
{
    public abstract class CommandManager
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual IEnumerable<string> Aliases { get; } = Array.Empty<string>();
        public virtual PermissionFlags Permissions { get; } = PermissionFlags.None;
        public virtual bool LogCommand { get; } = true;

        protected IMediator Mediator { get; }
        private IPlayerContainerReader PlayerContainer { get; }

        protected CommandManager(IMediator mediator, IPlayerContainerReader playerContainer)
        {
            Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            PlayerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        protected async Task<IPlayer?> GetPlayerAsync(string name, CancellationToken ct)
        {
            return await PlayerContainer.GetAllAsync(ct).FirstOrDefaultAsync(x => x.Name.Equals(name, StringComparison.Ordinal), ct);
        }

        protected async Task SendMessageAsync(IPlayer player, string message, CancellationToken ct)
        {
            await Mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, player, message), ct);
        }

        protected async Task SendServerMessageAsync(string message, CancellationToken ct)
        {
            await Mediator.Publish(new ServerMessageNotification(message), ct);
        }

        public virtual async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            await HelpAsync(player, alias, ct);
        }

        public virtual async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $@"Command ""{alias}"" is not functional!", ct);
        }
    }
}