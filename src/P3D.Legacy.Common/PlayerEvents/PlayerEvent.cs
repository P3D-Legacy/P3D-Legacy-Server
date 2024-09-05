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
    public sealed record UnknownEvent(string RawEvent) : PlayerEvent(PlayerEventType.Unknown);
    public sealed record AchievedEmblemEvent(string EmblemName) : PlayerEvent(PlayerEventType.AchievedEmblem);
    public sealed record DefeatedByTrainerEvent(string TrainerTypeAndName) : PlayerEvent(PlayerEventType.DefeatedByTrainer);
    public sealed record DefeatedByWildPokemonEvent(string PokemonName) : PlayerEvent(PlayerEventType.DefeatedByWildPokemon);
    public sealed record HostedABattleEvent(string TrainerName, string DefeatedTrainerName) : PlayerEvent(PlayerEventType.HostedABattle);
    public sealed record EvolvedPokemonEvent(string PokemonName, string EvolvedPokemonName) : PlayerEvent(PlayerEventType.EvolvedPokemon);
}