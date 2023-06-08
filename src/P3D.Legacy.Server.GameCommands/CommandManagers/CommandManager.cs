using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;
using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers
{
    internal abstract class CommandManager
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual IEnumerable<string> Aliases { get; } = Array.Empty<string>();
        public virtual PermissionTypes Permissions { get; } = PermissionTypes.None;
        public virtual bool LogCommand { get; } = true;

        protected ICommandDispatcher CommandDispatcher { get; }
        protected IQueryDispatcher QueryDispatcher { get; }
        protected IEventDispatcher EventDispatcher { get; }

        protected CommandManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));

            CommandDispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
            QueryDispatcher = serviceProvider.GetRequiredService<IQueryDispatcher>();
            EventDispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();
        }

        protected async Task<IPlayer?> GetPlayerAsync(string name, CancellationToken ct)
        {
            var players = await QueryDispatcher.DispatchAsync(new GetPlayersInitializedQuery(), ct);
            return players.FirstOrDefault(x => x.Name.Equals(name, StringComparison.Ordinal));
        }

        protected async Task SendMessageAsync(IPlayer player, string message, CancellationToken ct)
        {
            await EventDispatcher.DispatchAsync(new MessageToPlayerEvent(IPlayer.Server, player, message), ct);
        }

        protected async Task SendServerMessageAsync(string message, CancellationToken ct)
        {
            await EventDispatcher.DispatchAsync(new ServerMessageEvent(message), ct);
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