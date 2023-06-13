using P3D.Legacy.Common.PlayerEvents;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player
{
    internal class TriggerPlayerEventCommandManager : CommandManager
    {
        private enum EventType
        {
            AchievedEmblem,
            DefeatedByTrainer,
            DefeatedByWildPokemon,
            HostedABattle,
            EvolvedPokemon,
        }

        public override string Name => "triggerevent";
        public override string Description => "Trigger Player Event.";
        public override IEnumerable<string> Aliases => new[] { "te" };
        public override PermissionTypes Permissions => PermissionTypes.Debug;

        public TriggerPlayerEventCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (Enum.TryParse(arguments[0], true, out EventType eventType))
            {
                var @event = eventType switch
                {
                    EventType.AchievedEmblem => arguments.Length != 2 ? null : (PlayerEvent) new AchievedEmblemEvent(arguments[1]),
                    EventType.DefeatedByTrainer => arguments.Length != 2 ? null : (PlayerEvent) new DefeatedByTrainerEvent(arguments[1]),
                    EventType.DefeatedByWildPokemon => arguments.Length != 2 ? null : (PlayerEvent) new DefeatedByWildPokemonEvent(arguments[1]),
                    EventType.HostedABattle => arguments.Length != 3 ? null : (PlayerEvent) new HostedABattleEvent(arguments[1], arguments[2]),
                    EventType.EvolvedPokemon => arguments.Length != 3 ? null : (PlayerEvent) new EvolvedPokemonEvent(arguments[1], arguments[2]),
                    _ => null
                };
                if (@event is null)
                    await SendMessageAsync(player, "Invalid arguments given.", ct);
                else
                    await EventDispatcher.DispatchAsync(new PlayerTriggeredEventEvent(player, @event), ct);
            }
            else
            {
                await SendMessageAsync(player, $"Event '{arguments[0]}' was not found!", ct);
            }
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <eventType> [args]", ct);
        }
    }
}