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
        private static readonly char[] DefeatedByTrainerMarker = "got defeated by".ToCharArray();

        public static bool TryParse(in ReadOnlySpan<char> message, [NotNullWhen(true)] out Event? eventType)
        {
            if (message.StartsWith(AchievedEmblemMarker))
            {
                var emblem = message.Slice(AchievedEmblemMarker.Length, message.Length - AchievedEmblemMarker.Length - 2);

                eventType = new AchievedEmblemEvent(emblem.ToString());
                return true;
            }
            if (message.StartsWith(HostedABattleMarker))
            {
                var span = message.Slice(HostedABattleMarker.Length);
                var idx = span.IndexOf(HostedABattleMarker2);
                var defeatedTrainer = span.Slice(0, idx);
                var trainer = span.Slice(idx + HostedABattleMarker2.Length, span.Length - idx - HostedABattleMarker2.Length - 2);

                eventType = new HostedABattleEvent(trainer.ToString(), defeatedTrainer.ToString());
                return true;
            }
            if (message.StartsWith(EvolvedPokemonMarker))
            {
                var span = message.Slice(EvolvedPokemonMarker.Length);
                var idx = span.IndexOf(EvolvedPokemonMarker2);
                var pokemonName = span.Slice(0, idx);
                var evolvedPokemonName = span.Slice(idx + EvolvedPokemonMarker2.Length, span.Length - idx - EvolvedPokemonMarker2.Length - 1);

                eventType = new EvolvedPokemonEvent(pokemonName.ToString(), evolvedPokemonName.ToString());
                return true;
            }
            if (message.StartsWith(DefeatedByWildPokemonMarker))
            {
                var pokemonName = message.Slice(DefeatedByWildPokemonMarker.Length, message.Length - DefeatedByWildPokemonMarker.Length - 1);

                eventType = new DefeatedByWildPokemonEvent(pokemonName.ToString());
                return true;
            }
            if (message.StartsWith(DefeatedByTrainerMarker))
            {
                var trainerTypeAndName = message.Slice(DefeatedByTrainerMarker.Length, message.Length - DefeatedByTrainerMarker.Length - 1);

                eventType = new DefeatedByTrainerEvent(trainerTypeAndName.ToString());
                return true;
            }

            eventType = null;
            return false;
        }
    }
}