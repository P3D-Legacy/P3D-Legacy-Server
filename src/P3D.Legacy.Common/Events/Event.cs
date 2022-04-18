using System.Text.Json.Serialization;

namespace P3D.Legacy.Common.Events
{
    public abstract partial record Event([JsonDiscriminator] EventType EventType);
    public record AchievedEmblemEvent(string Emblem) : Event(EventType.AchievedEmblem);
    public record DefeatedByTrainerEvent(string TrainerTypeAndName) : Event(EventType.DefeatedByTrainer);
    public record DefeatedByWildPokemonEvent(string PokemonName) : Event(EventType.DefeatedByWildPokemon);
    public record HostedABattleEvent(string Trainer, string DefeatedTrainer) : Event(EventType.HostedABattle);
    public record EvolvedPokemonEvent(string PokemonName, string EvolvedPokemonName) : Event(EventType.EvolvedPokemon);
}
