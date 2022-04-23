using System;
using System.Diagnostics.CodeAnalysis;

namespace P3D.Legacy.Common.Events
{
    public static class EventParser
    {
        private static readonly char[] AchievedEmblemMarker = "achieved the emblem \"".ToCharArray();
        private static readonly char[] HostedABattleMarker = "hosted a battle: \"Player ".ToCharArray();
        private static readonly char[] HostedABattleMarker2 = " got defeated by Player ".ToCharArray();
        private static readonly char[] EvolvedPokemonMarker = "evolved their ".ToCharArray();
        private static readonly char[] EvolvedPokemonMarker2 = " into a ".ToCharArray();
        private static readonly char[] DefeatedByWildPokemonMarker = "got defeated by a wild ".ToCharArray();
        private static readonly char[] DefeatedByTrainerMarker = "got defeated by ".ToCharArray();

        public static bool TryParse(in ReadOnlySpan<char> message, [NotNullWhen(true)] out Event? eventType)
        {
            var eventMessage = message;

            if (eventMessage.StartsWith(AchievedEmblemMarker))
            {
                var emblem = eventMessage.Slice(AchievedEmblemMarker.Length, eventMessage.Length - AchievedEmblemMarker.Length - 2);

                eventType = new AchievedEmblemEvent(emblem.ToString());
                return true;
            }
            if (eventMessage.StartsWith(HostedABattleMarker))
            {
                var span = eventMessage.Slice(HostedABattleMarker.Length);
                var idx = span.IndexOf(HostedABattleMarker2);
                var defeatedTrainer = span.Slice(0, idx);
                var trainer = span.Slice(idx + HostedABattleMarker2.Length, span.Length - idx - HostedABattleMarker2.Length - 2);

                eventType = new HostedABattleEvent(trainer.ToString(), defeatedTrainer.ToString());
                return true;
            }
            if (eventMessage.StartsWith(EvolvedPokemonMarker))
            {
                var span = eventMessage.Slice(EvolvedPokemonMarker.Length);
                var idx = span.IndexOf(EvolvedPokemonMarker2);
                var pokemonName = span.Slice(0, idx);
                var evolvedPokemonName = span.Slice(idx + EvolvedPokemonMarker2.Length, span.Length - idx - EvolvedPokemonMarker2.Length - 1);

                eventType = new EvolvedPokemonEvent(pokemonName.ToString(), evolvedPokemonName.ToString());
                return true;
            }
            if (eventMessage.StartsWith(DefeatedByWildPokemonMarker))
            {
                var pokemonName = eventMessage.Slice(DefeatedByWildPokemonMarker.Length, eventMessage.Length - DefeatedByWildPokemonMarker.Length - 1);

                eventType = new DefeatedByWildPokemonEvent(pokemonName.ToString());
                return true;
            }
            if (eventMessage.StartsWith(DefeatedByTrainerMarker))
            {
                var trainerTypeAndName = eventMessage.Slice(DefeatedByTrainerMarker.Length, eventMessage.Length - DefeatedByTrainerMarker.Length - 1);

                eventType = new DefeatedByTrainerEvent(trainerTypeAndName.ToString());
                return true;
            }

            eventType = new UnknownEvent(eventMessage.ToString());
            return true;
        }

        public static string AsText(Event @event) => @event switch
        {
            AchievedEmblemEvent achievedEmblemEvent => $"achieved the emblem {achievedEmblemEvent.Emblem}!",
            DefeatedByTrainerEvent defeatedByTrainerEvent => $"got defeated by {defeatedByTrainerEvent.TrainerTypeAndName}!",
            DefeatedByWildPokemonEvent defeatedByWildPokemonEvent => $"got defeated by a wild {defeatedByWildPokemonEvent.PokemonName}!",
            EvolvedPokemonEvent evolvedPokemonEvent => $"evolved their {evolvedPokemonEvent.PokemonName} into a {evolvedPokemonEvent.EvolvedPokemonName}!",
            HostedABattleEvent hostedABattleEvent => $"hosted a battle: \"Player {hostedABattleEvent.DefeatedTrainer} got defeated by Player {hostedABattleEvent.Trainer}\"!",
            UnknownEvent unknownEvent => unknownEvent.RawEvent,
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };
    }
}