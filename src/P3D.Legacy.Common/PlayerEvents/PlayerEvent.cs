using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("P3D.Legacy.Server.CommunicationAPI")]

namespace P3D.Legacy.Common.PlayerEvents
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "eventType")]
    [JsonDerivedType(typeof(UnknownEvent), (int) PlayerEventType.Unknown)]
    [JsonDerivedType(typeof(AchievedEmblemEvent), (int) PlayerEventType.AchievedEmblem)]
    [JsonDerivedType(typeof(DefeatedByTrainerEvent), (int) PlayerEventType.DefeatedByTrainer)]
    [JsonDerivedType(typeof(DefeatedByWildPokemonEvent), (int) PlayerEventType.DefeatedByWildPokemon)]
    [JsonDerivedType(typeof(HostedABattleEvent), (int) PlayerEventType.HostedABattle)]
    [JsonDerivedType(typeof(EvolvedPokemonEvent), (int) PlayerEventType.EvolvedPokemon)]
    public abstract record PlayerEvent
    {
        public PlayerEventType EventType { get; set; }
    };
    public record UnknownEvent(string RawEvent) : PlayerEvent;
    public record AchievedEmblemEvent(string Emblem) : PlayerEvent;
    public record DefeatedByTrainerEvent(string TrainerTypeAndName) : PlayerEvent;
    public record DefeatedByWildPokemonEvent(string PokemonName) : PlayerEvent;
    public record HostedABattleEvent(string Trainer, string DefeatedTrainer) : PlayerEvent;
    public record EvolvedPokemonEvent(string PokemonName, string EvolvedPokemonName) : PlayerEvent;
}
