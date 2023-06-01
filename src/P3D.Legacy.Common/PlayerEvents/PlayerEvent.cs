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
    public abstract record PlayerEvent([property: JsonIgnore] PlayerEventType EventType);
    public record UnknownEvent(string RawEvent) : PlayerEvent(PlayerEventType.Unknown);
    public record AchievedEmblemEvent(string Emblem) : PlayerEvent(PlayerEventType.AchievedEmblem);
    public record DefeatedByTrainerEvent(string TrainerTypeAndName) : PlayerEvent(PlayerEventType.DefeatedByTrainer);
    public record DefeatedByWildPokemonEvent(string PokemonName) : PlayerEvent(PlayerEventType.DefeatedByWildPokemon);
    public record HostedABattleEvent(string Trainer, string DefeatedTrainer) : PlayerEvent(PlayerEventType.HostedABattle);
    public record EvolvedPokemonEvent(string PokemonName, string EvolvedPokemonName) : PlayerEvent(PlayerEventType.EvolvedPokemon);
}
